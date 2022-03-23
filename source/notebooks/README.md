# Notebooks

.NET interactive notebooks and dotnet spark requires execution in the devcontainer.
The default devcontainer is, however, not sufficient.

In order to run the notebooks the following steps must be followed:

1. Copy the files in `./devcontainer` to folder `<repo-root>/devcontainer` (overwriting the existing files)
2. Run the repository in the devcontainer in VSCode using the `Remote - Containers` extension. Read more [here](https://code.visualstudio.com/docs/remote/containers)
