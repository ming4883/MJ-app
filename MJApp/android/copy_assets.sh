echo pwd is: $(pwd)
echo removing old assets...
if [ -d assets ]; then
    rm -r assets
fi

if [ ! -d assets ]; then
    mkdir assets
fi

echo copying assets...
cp ../media/MJApp.gles assets
cp ../../Crender/media/Common.gles assets
