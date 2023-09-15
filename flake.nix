{
  #inputs.nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
  inputs.nixpkgs.url = "path:/home/root@Rory/git/Matrix/MatrixRoomUtils/nixpkgs";
  inputs.flake-utils.url = "github:numtide/flake-utils";

  outputs = { self, nixpkgs, flake-utils }:
      let
        pkgs = nixpkgs.legacyPackages.x86_64-linux;
      in
      {
         packages.x86_64-linux = {
            web = pkgs.buildDotnetModule rec {
              pname = "MatrixRoomUtils.Web-v${version}";
              version = "1";
              dotnet-sdk = pkgs.dotnet-sdk_7;
              dotnet-runtime = pkgs.dotnet-aspnetcore_7;
              src = ./.;
              projectFile = [
                "MatrixRoomUtils.Web/MatrixRoomUtils.Web.csproj"
               ];
              nugetDeps = MatrixRoomUtils.Web/deps.nix;
              #nativeBuildInputs = with pkgs; [
              #  pkg-config
              #];
            };
        };
    };
}
