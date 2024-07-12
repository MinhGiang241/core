namespace CommonLibCore.Model
{
    public class PagingModel : Response
    {
        public PagingModel()
            : base() { }

        public PagingModel(object data)
            : base(data) { }

        public PagingModel(int status, string code, string message)
            : base(status, code, message) { }

        public int Pages { get; set; }
        public int Page { get; set; } //currentpage
        public int Records { get; set; }
        public string Extra { get; set; }
    }
}
