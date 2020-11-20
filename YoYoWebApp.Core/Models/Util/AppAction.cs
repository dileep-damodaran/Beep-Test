namespace YoYoWebApp.Core.Models.Util
{
    public class AppAction
    {
        public AppAction(bool isSucceeded, dynamic data)
        {
            this.IsSucceeded = isSucceeded;
            this.Result = data;
        }
        public bool IsSucceeded { get; set; }


        public dynamic Result { get; set; }
    }
}
