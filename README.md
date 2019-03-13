# convertts
Convert mpeg2 .ts files to .mp4

.net core console app. Will run on Windows/Mac/Linux with the .Net Core runtime installed. ffmpeg must be in your path for this to work - or you can clone and change the source. Yet Another ffpeg wrapper ...

crawls files and directories starting at locations passed in as arguments:<p>
dotnet convertts.dll "D:\TV\Some Show (2018)" "D:\TV\Some Other Directory"

runs the ffmpeg process with belownormal priority
