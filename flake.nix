{
  inputs.nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
  inputs.flake-utils.url = "github:numtide/flake-utils";

  outputs = { self, nixpkgs, flake-utils }:
      let
        pkgs = nixpkgs.legacyPackages.x86_64-linux;
      in
      {
         packages.x86_64-linux = {
            bots = pkgs.buildDotnetModule rec {
              pname = "botcore-v${version}";
              version = "4";
              dotnet-sdk = pkgs.dotnet-sdk_7;
              dotnet-runtime = pkgs.dotnet-runtime_7;
              src = ./.;
              projectFile = [
                "BotCore.Runner/BotCore.Runner.csproj"
                "BotCore.SystemdServiceInvoker/BotCore.SystemdServiceInvoker.csproj"
               ];
              runtimeDeps = with pkgs; [ yt-dlp ];
              nugetDeps = ./deps.nix;
              #nativeBuildInputs = with pkgs; [
              #  pkg-config
              #];
            };
            frontend = pkgs.buildDotnetModule rec {
              pname = "botcore-v${version}";
              version = "4";
              dotnet-sdk = pkgs.dotnet-sdk_7;
              dotnet-runtime = pkgs.dotnet-aspnetcore_7;
              src = ./.;
              projectFile = [
                "BotCore.Web.Legacy/BotCore.Web.Legacy.csproj"
               ];
              nugetDeps = ./deps.nix;
              #nativeBuildInputs = with pkgs; [
              #  pkg-config
              #];
            };
            dataupdater = pkgs.buildDotnetModule rec {
              pname = "botcore-v${version}";
              version = "4";
              dotnet-sdk = pkgs.dotnet-sdk_7;
              dotnet-runtime = pkgs.dotnet-runtime_7;
              src = ./.;
              projectFile = [
                "BotCore.DataUpdater/BotCore.DataUpdater.csproj"
               ];
              nugetDeps = ./deps.nix;
              #nativeBuildInputs = with pkgs; [
              #  pkg-config
              #];
            };
            all = pkgs.buildDotnetModule rec {
              pname = "botcore-v${version}";
              version = "4";
              dotnet-sdk = pkgs.dotnet-sdk_7;
              dotnet-runtime = pkgs.dotnet-runtime_7;
              src = ./.;
              projectFile = "DiscordBots.sln";
              nugetDeps = ./deps.nix;
              #nativeBuildInputs = with pkgs; [
              #  pkg-config
              #];
            };
        };
        modules = {
          users = {
            users.users.botcore = {
              isSystemUser = true;
              home = "/var/lib/botcore";
              createHome = true;
              group = "botcore";
              extraGroups = [ "video" ];
            };
            users.groups.botcore = {};
            security.polkit.extraConfig = ''
              polkit.addRule(function(action, subject) {
                if (action.id == "org.freedesktop.systemd1.manage-units" &&
                    action.lookup("unit").startsWith("botcore.") &&
                    subject.user == "botcore") {
                  return polkit.Result.YES;
                }
              });
              '';
          };
          bots = {
              systemd.services = {
                  "botcore.bot@" = {
                    serviceConfig = {
                      ExecStart = "${self.packages.x86_64-linux.bots}/bin/BotCore.Runner %i";
                      Restart = "always";
                      RestartSec = "5";
                      User = "botcore";
                    };
                  };
                  "botcore.systemdserviceinvoker" = {
                    wantedBy = [ "multi-user.target" ];
                    serviceConfig = {
                      ExecStart = "${self.packages.x86_64-linux.bots}/bin/BotCore.SystemdServiceInvoker";
                      User = "botcore";
                    };
                  };
              };
          };
            frontend = {
                systemd.services = {
                    "botcore.web" = {
                        wantedBy = [ "multi-user.target" ];
                        serviceConfig = {
                            ExecStart = "${self.packages.x86_64-linux.frontend}/bin/BotCore.Web.Legacy";
                            Restart = "always";
                            RestartSec = "5";
                            User = "botcore";
                        };
                    };
                };
            };
            dataupdater = {
                systemd.services = {
                    "botcore.dataupdater" = {
                        wantedBy = [ "multi-user.target" ];
                        serviceConfig = {
                            ExecStart = "${self.packages.x86_64-linux.dataupdater}/bin/BotCore.DataUpdater";
                            Restart = "always";
                            RestartSec = "15min";
                            User = "botcore";
                        };
                    };
                };
            };
        };
    };
}
