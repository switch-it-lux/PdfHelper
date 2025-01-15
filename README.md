# Sitl.PdfHelper
Sitl.PdfHelper is a standard .Net 2.0 library for performing simple basic PDF manipulations such as:
- Spliting/merging PDF document.
- Converting image/email to PDF document.
- Adding basic elements (text, barcode, image) to PDF document.
- Searching for text/barcode locations.
- Appling HOCR texts to an OCRed PDF document to make it searchable.
- Generating and assigning thumbnail to PDF document.
- And more.

## Installation
Either checkout this Github repository or install via NuGet Package Manager.
If you want to use NuGet just search for "Sitl.PdfHelper" (nuget.org) or run the following command in the NuGet Package Manager console:
```bash
PM> Install-Package Sitl.PdfHelper
```
## Usage
Instanciate the Sitl.Pdf.PdfHelper class and call any methods you need. 

#### Sample 1: Split PDF document by max size
```csharp
using var pdfHelper = new Sitl.Pdf.PdfHelper("c:\\temp\\sample.pdf");
var docs = pdfHelper.SplitByMaxSize(100000); //A file should not exceed 100Kb
for (int i = 0; i < docs.Count; i++) {
    File.WriteAllBytes($"c:\\temp\\sample ({i + 1}).pdf", docs[i]);
}
```

#### Sample 2: Convert email to PDF document
```csharp
using var pdfHelper = await Sitl.Pdf.PdfHelper.FromEmailAsync("c:\\temp\\sample.eml", EmailType.Mime, PageSizes.A4, includeAttachments: true);
pdfHelper.Save($"c:\\temp\\sample.pdf");
```

#### Sample 3: Generate thumbnail image for first page and store it to the PDF document
```csharp
using var pdfHelper = new PdfHelper("c:\\temp\\sample.pdf");
var img = pdfHelper.GenerateThumbnail(page: 1, width: 200, whiteBackground: true);
pdfHelper.SetThumbnail(img, 1);
pdfHelper.Save("c:\\temp\\sample with thumbnail.pdf");
```

#### Sample 4: Add and then find QR code
```csharp
// Adding a QR code to a new PDF
using var pdfHelper = new Sitl.Pdf.PdfHelper(PageSizes.A4);
pdfHelper.AddElements(new PdfQrcodeElement {
    Page = 1,
    X = 10,
    Y = 110,
    Anchor = PdfElementAnchor.BaselineLeft,
    Color = System.Drawing.Color.Black,
    Size = 100,
    Text = "http://www.switch.lu"
});
pdfHelper.Save($"c:\\temp\\qr.pdf");

// Finding QR code
var results = pdfHelper.GetBarcodeLocations(stopAfterXMatch: 1);
if (results.Length > 0) {
    Debug.WriteLine("Text: " + results[0].Text);
    Debug.WriteLine("Page: " + results[0].Location.Page);
    Debug.WriteLine("Location: " + results[0].Location.XLeft + "," + results[0].Location.YBottom);
}
```

Find more usage examples in the folder '\src\Tests\PdfHelperTests'.

## Legal information and credits
Sitl.PdfHelper is a project by [Switch IT](https://www.switch.lu) licensed under the [GNU Affero General Public License](https://www.gnu.org/licenses/).

Third-party components:
- .Net Foundation components by Microsoft ([https://github.com/microsoft](https://github.com/microsoft), [https://github.com/dotnet](https://github.com/dotnet)).
  Licensed under the MIT License.
- HtmlAgilityPack by ZZZ Projects, Simon Mourrier, Jeff Klawiter and Stephan Grell ([https://github.com/zzzprojects/html-agility-pack](https://github.com/zzzprojects/html-agility-pack)).
  Licensed under the MIT License.
- iText7 by Apryse Software ([https://github.com/itext/itext-dotnet](https://github.com/itext/itext-dotnet)).
  Licensed under the GNU Affero General Public License.
- MailKit and MimeKit by Jeffrey Stedfast ([https://github.com/jstedfast/MailKit](https://github.com/jstedfast/MailKit)).
  Licensed under the MIT License.
- MsgReader by Kees van Spelde ([https://github.com/Sicos1977/MSGReader](https://github.com/Sicos1977/MSGReader)).
  Licensed under the MIT License.
- PDFtoImage by David Sungaila ([https://github.com/sungaila/PDFtoImage](https://github.com/sungaila/PDFtoImage)).
  Licensed under the MIT License.
- Newtonsoft.Json by James Newton-King ([https://github.com/JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)).
  Licensed under the MIT License.
- ZXing.Net by Michael Jahn ([https://github.com/micjahn/ZXing.Net](https://github.com/micjahn/ZXing.Net)).
  Licensed under the Apache 2.0 license.
