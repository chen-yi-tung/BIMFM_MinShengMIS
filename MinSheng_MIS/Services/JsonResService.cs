namespace MinSheng_MIS.Service
{
    public enum ResState
    {
        Success,
        Failed,
        Unauthorized,
        Expired
    }
    public class JsonResService
    {
        public string State { get; private set; }

        public ResState AccessState
        {
            set
            {
                this.State = value.ToString();
            }
        }
        public string ErrorMessage { get; set; }
        public object Datas { get; set; }
    }
}
