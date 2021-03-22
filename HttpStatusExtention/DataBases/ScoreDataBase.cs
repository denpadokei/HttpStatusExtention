using HttpSiraStatus.Util;
using System.Threading;
using System.Threading.Tasks;

namespace HttpStatusExtention.DataBases
{
    public class ScoreDataBase
    {
        public static ScoreDataBase Instance { get; } = new ScoreDataBase();
        public static JSONNode Songs { get; private set; } = new JSONObject();

        private const string URI_PREFIX = "https://cdn.pulselane.dev/";
        private const string FILE_NAME = "raw_pp.json";

        public bool Init { get; set; } = false;

        private ScoreDataBase()
        {

        }

        public async Task Initialize()
        {
            var jsonObject = await WebClient.GetAsync($"{URI_PREFIX}{FILE_NAME}", CancellationToken.None);
            Songs = jsonObject.ConvertToJsonNode();
            this.Init = true;
        }
    }
}
