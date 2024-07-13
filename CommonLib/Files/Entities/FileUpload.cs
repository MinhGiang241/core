namespace CommonLibCore.CommonLib.Files.Entities
{
    public class FileUpload
    {
        public FileUpload() { }

        public FileUpload(string filename) => this.Filename = filename;

        public string? Id { get; set; }
        public string? Filename { get; set; }
        public string? Minetype { get; set; }
        public string? Path { get; set; }
    }
}
