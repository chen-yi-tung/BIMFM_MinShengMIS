namespace MinSheng_MIS.Services
{
    public enum ResState
    {
        Success,
        Failed,
        Unauthorized,
        Expired
    }
    public class JsonResService<T>
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
        public T Datas { get; set; }
    }
}
