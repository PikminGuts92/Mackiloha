BUILD_MODE="Release"
BUILD_VERSION=${1:-"0.0.0.0"}
OUTPUT_PATH="./BUILD"
ZIP_PATH="$OUTPUT_PATH/Mackiloha_v$BUILD_VERSION-win-x64.zip"

# Clear previous build
echo ">> Clearing old files"
rm $OUTPUT_PATH -rf

# Build + publish projects
echo ">> Building solution"
dotnet publish Src/UI/**/*.csproj -c $BUILD_MODE -o $OUTPUT_PATH -p:Version=$BUILD_VERSION --self-contained=false -r=win-x64 # Windows

# Delete debug + config files
echo ">> Removing debug files"
rm ./$OUTPUT_PATH/*.config -f
rm ./$OUTPUT_PATH/*.pdb -f

# Copy licences + README
echo ">> Copying licenses and README"
cp ./LICENSE ./$OUTPUT_PATH/LICENSE -f
cp ./THIRDPARTY ./$OUTPUT_PATH/THIRDPARTY -f
cp ./README.md ./$OUTPUT_PATH/README.md -f

# Zip everything up
echo ">> Zipping everything up"
zip $ZIP_PATH $OUTPUT_PATH -jr