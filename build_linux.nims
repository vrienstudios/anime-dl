#[
cd ../libs/ADLCore
rm -rf /home/shu/.nimble/pkgs2/ADLCore-0*
nimble install -y
cd ../..
nim c -d:ssl --threads:on -f ./animeDL.nim
]#

if defined(Linux):
  withDir "libs/ADLCore":
    exec "rm -rf $HOME/.nimble/pkgs/ADLCore-0*"
    exec "nimble install -y"

  exec "nim c -d:ssl --threads:on -f ./animeDL.nim"
  exec "mkdir -p ./bin/amd64"
  mvFile "./animeDL", "./bin/amd64/animeDL"
else:
  echo "This can not be ran on Windows, yet"