# ISADotNet_docs

WebPage: https://nfdi4plants.github.io/ISADotNet-docs/
ISADotNet: https://github.com/nfdi4plants/ISADotNet

## For developers

After initializing this template please replace two placeholder variables:

1. `src/generators/layout.fsx`, replace `"placeholder"` in `let baseUrl = "placeholder"` with the correct base url. See example above.
2. `src/loaders/docsloader.fsx`, replace `"placeholder"` in `let productionBasePath = "placeholder"` with the repository name if for example hosted on GitHub pages.

Then run the following commands to get started: 

1. `dotnet tool restore`, will restore local dotnet tools _fornax_ and _paket_.
2. `dotnet paket install`, will download the Nfdi4Plants.Fornax library.
3. `cd src` -> `dotnet fornax watch`

Done! 🎉 You can now open your static website on http://127.0.0.1:8080

## Docs 

Check out the docs for usage [here](https://nfdi4plants.github.io/web-components-docs/docs/SupportedStaticSiteGenerators.html#fornax).
