// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [// Standard resources
    /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/, /\.webmanifest$/, /* Extra known-static paths */
    /\/_matrix\/media\/.{2}\/download\//, /api\.dicebear\.com\/6\.x\/identicon\/svg/];
const offlineAssetsExclude = [/^service-worker\.js$/];

// Replace with your base path if you are hosting on a subfolder. Ensure there is a trailing '/'.
const base = "/";
const baseUrl = new URL(base, self.origin);
const manifestUrlList = self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href);

async function onInstall(event) {
    console.info('Service worker: Install');

    // Activate the new service worker as soon as the old one is retired.
    self.skipWaiting();

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, {cache: 'no-cache'})); /* integrity: asset.hash */
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // For all navigation requests, try to serve index.html from cache,
        // unless that request is for an offline resource.
        // If you need some URLs to be server-rendered, edit the following check to exclude those URLs
        const shouldServeIndexHtml = event.request.mode === 'navigate'
            && !manifestUrlList.some(url => url === event.request.url);

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const shouldCache = offlineAssetsInclude.some(pattern => pattern.test(request.url));

        const cache = await caches.open(cacheName);

        if (request !== 'index.html' && request.url.endsWith("_framework/dotnet.js")) {
            // return `_framework/dotnet.<hash>.js` from cache to avoid integrity errors
            const dotnetJsUrl = manifestUrlList.find(url => /_framework\/dotnet\.[a-z0-9]+\.js$/.test(url));
            if (dotnetJsUrl) {
                cachedResponse = await cache.match(dotnetJsUrl);
                if (cachedResponse) {
                    console.log("Service worker caching: serving dotnet.js from cache: ", dotnetJsUrl);
                    return cachedResponse;
                }
            } else console.warn("Service worker caching: could not find dotnet.hash.js in manifest", {request, manifestUrlList});
        }

        cachedResponse = await cache.match(request);
        let exception;
        let fetched;
        if (!cachedResponse && shouldCache) {
            console.log("Service worker caching: fetching ", request.url)
            try {
                fetched = true;
                await cache.add(request);
                cachedResponse = await cache.match(request);
            } catch (e) {
                exception = e;
                console.error("cache.add error: ", e, request.url)
            }
        }
        let consoleLog = {
            fetched, shouldCache, request, exception, cachedResponse, url: request.url,
        }
        Object.keys(consoleLog).forEach(key => consoleLog[key] == null && delete consoleLog[key])
        if (consoleLog.exception)
            console.log("Service worker caching: ", consoleLog)
    }

    return cachedResponse || fetch(event.request);
}
