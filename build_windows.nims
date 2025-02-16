if defined(Linux):
  withDir "libs/ADLCore":
    exec "rm -rf $HOME/.nimble/pkgs/ADLCore-0*"
    exec "nimble install -y"
  exec "nim c -d:ssl -d:mingw --threads:on -f ./animeDL.nim"
  exec "mkdir -p ./bin/win64/"
  mvFile "./animeDL.exe", "./bin/win64/animeDL.exe"
else:
  echo "build normally, don't run this if not on linux"