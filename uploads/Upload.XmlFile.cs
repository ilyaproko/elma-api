
namespace Upload.XmlFile;

class UploadXml
{
    public UploadXml()
    {
    }

    public void UploadEmployeesSubadi(string pathDirectory) 
    {
        if (Directory.Exists(pathDirectory))
            throw new Exception($"Directory '{pathDirectory}' wasn't found");

        var files = Directory.GetFiles(pathDirectory) 
            .Where(file => Path.GetExtension(file) == ".xml" 
                && Path.GetFileName(file).Contains("sibadi_zkgu_"));

        var filesEmployeesSubadi = files.Select(file =>
        {
            var dateStr = Path.GetFileName(file).Substring(12, 12);
            var day = dateStr.Substring(0, 2);
            var month = dateStr.Substring(2, 2);
            var year = "20" + dateStr.Substring(4, 2);

            return new FileAndDate
            {
                Name = Path.GetFileName(file),
                dateCreated = DateTime.ParseExact(day + "/" + month + "/" + year, format: "dd/MM/yyyy", null)
            };
        }).OrderByDescending(f => f.dateCreated);

        var lastUploadedFile = filesEmployeesSubadi.First();



    }

}

class FileAndDate
{
    public string Name { get; set; }
    public DateTime dateCreated { get; set; }
}