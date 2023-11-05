BUILD_VERSION="1.2.0"
RUNTIME=""
BUILD_MODE="Release"
OUTPUT_PATH="./BUILD"

# Read in args
while [ $# -gt 0 ]; do
    case "$1" in
        -v) shift && BUILD_VERSION=$1 ;;
        -r) shift && RUNTIME=$1 ;;
        -o) shift && OUTPUT_PATH=$1 ;;
        *) echo "Error: Unexpected argument \"$1\"" && exit 1
    esac
    shift
done

# Detect runtime if not set
if [ -z $RUNTIME ]
then
    case "$(uname -s)" in
        Linux)
            RUNTIME="linux-x64"
            echo "Linux detected, using \"$RUNTIME\" runtime"
            ;;
        Darwin)
            RUNTIME="osx-x64"
            echo "Mac OSX detected, using \"$RUNTIME\" runtime"
            ;;
        *)
            # Default to Windows
            RUNTIME="win-x64"
            echo "Windows detected, using \"$RUNTIME\" runtime"
            ;;
    esac
fi

# Clear previous build
echo ">> Clearing old files"
rm $OUTPUT_PATH -rf

# Get projects to build
PROJECTS=$(find Src/UI/**/*.csproj)

# Build + publish projects
echo ">> Building solution"
for proj in $PROJECTS; do
    dotnet publish $proj -c $BUILD_MODE -o $OUTPUT_PATH -p:Version=$BUILD_VERSION -r=$RUNTIME
done

# Delete debug + config files
echo ">> Removing debug files"
rm -f ./$OUTPUT_PATH/*.config
rm -f ./$OUTPUT_PATH/*.pdb
rm -f ./$OUTPUT_PATH/*.dbg # Mac debug

# Copy licences + README
echo ">> Copying licenses and README"
cp -f ./LICENSE ./$OUTPUT_PATH/LICENSE
cp -f ./THIRDPARTY ./$OUTPUT_PATH/THIRDPARTY
cp -f ./README.md ./$OUTPUT_PATH/README.md