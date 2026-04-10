using System;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class questionData
{
    public string title;
    public string question;
    public List<string> answers;
    public List<string> codes;
}

public class jobData
{
    public string man;
    public string woman;
    public string keywordMan;
    public string keywordWoman;
    public string summaryMan;
    public string summaryWoman;
    public string skill;
    public string attitude;
    public string code;
}

public static class XmlManager
{
    #region SettingXml

    public class ProgramConfig
    {
        public int captureWait;
        public int askWait;
        public int resultWait;
    }
    public static ProgramConfig program;

    public class HttpConfig
    {
        public List<string> endPoint;
    }
    public static HttpConfig http;

    public class RedisConfig
    {
        public List<string> endPoint;
        public int port;
        public string password;
        public string channel;
    }
    public static RedisConfig redis;

    public class CameraConfig
    {
        public string webRtcUrl;

        public int rotateAngle;

        public bool isFlip;
    }
    public static CameraConfig camera;

    #endregion

    #region LanguageXml

    public class LanguageConfig
    {
        // 공통 텍스트
        public string next;
        public string back;
        public string home;
        // =========

        // 질문 화면
        public string ready;

        public string gender;
        public string man;
        public string woman;
        public string other;

        public List<string> qTitles;
        public List<questionData> qnas;

        // =========

        // 촬영 화면
        public string camReady1;
        public string camReady2;
        public string lookUp;
        public string recapture;
        public string capture;
        // ========

        // AI 로딩 화면
        public string drawing;
        // ==========

        // 결과 화면
        public string resultTitle;
        public string qrSave;
        public string exit;
        public string qrTitle;
        public List<jobData> jobs;

        // 에러 메시지
        public string invalidSelection;
        public string aiPcCommunicationFailed;
        public string nlogInitFailed;
        public string dataFileLoadFailed;
        public string communicationFailed;
        public string programInitFailed;
        public string faceRecognitionFailed;
        public string aiPcError;
    }
    public static LanguageConfig lang;

    #endregion

    private static string _setPath;
    private static string _langPath;

    private static XDocument _setXml;
    private static XElement _setRoot;
    private static XDocument _langXml;
    private static XElement _langRoot;

    static public async Task InitXml()
    {
        _setPath = await StreamingWorker.GetFile("Setting.xml");
        _langPath = await StreamingWorker.GetFile("Language.xml");

        InitConfig();
    }

    public static void InitConfig()
    {
        try
        {
            // Setting Xml
            _setXml = XDocument.Load(_setPath);
            _setRoot = _setXml.Element("Root");


            program = new ProgramConfig
            {
                captureWait = int.TryParse(_setRoot.Element("Program").Element("captureWait").Value, out int tmpCaptureWait) ? tmpCaptureWait : 5,
                askWait = int.TryParse(_setRoot.Element("Program").Element("askWait").Value, out int tmpAskWait) ? tmpAskWait : 180,
                resultWait = int.TryParse(_setRoot.Element("Program").Element("resultWait").Value, out int tmpResultWait) ? tmpResultWait : 120,
            };

            http = new HttpConfig
            {
                endPoint = _setRoot.Element("Http").Element("Endpoint").Elements().Select(o => o.Value).ToList()
            };

            redis = new RedisConfig
            {
                endPoint = _setRoot.Element("Redis").Element("Endpoint").Elements().Select(o => o.Value).ToList(),
                port = int.TryParse(_setRoot.Element("Redis").Element("Port").Value, out int tmpPort) ? tmpPort : -1,
                password = _setRoot.Element("Redis").Element("Password").Value ?? null,
                channel = _setRoot.Element("Redis").Element("Channel").Value
            };

            camera = new CameraConfig
            {
                webRtcUrl = _setRoot.Element("Camera").Element("WebRtcUrl").Value,
                rotateAngle = int.TryParse(_setRoot.Element("Camera").Element("Rotate_angle").Value, out int tmpAngle) ? tmpAngle : 0,
                isFlip = bool.TryParse(_setRoot.Element("Camera").Element("Flip").Value, out bool tmpFlip) ? tmpFlip : false
            };
            // ==================
        }
        catch (Exception ex)
        {
            NLogManager.Error($"SETTING xml init error: {ex}");

            throw;
        }

        // Language Xml
        SetLangXml();
        // ==================

        NLogManager.Info("XML Init Completed.");
    }

    public static void SetLangXml(NationCode nc = NationCode.ko)
    {
        try
        {
            _langXml = XDocument.Load(_langPath);
            _langRoot = _langXml.Element("Root");

            XElement general = _langRoot.Element("general");
            XElement main = _langRoot.Element("main");
            XElement ask = _langRoot.Element("ask");
            XElement cam = _langRoot.Element("cam");
            XElement loading = _langRoot.Element("loading");
            XElement result = _langRoot.Element("result");
            XElement error = _langRoot.Element("error");

            LanguageManager.SetNationCode(nc);
            string natCode = LanguageManager.GetNationCode();

            // 질의응답 세트
            var tempQnas = new List<questionData>();

            var tempQTitles = ask.Element("titles").Elements("title").Select(o => o.Element(natCode).Value).ToList();

            foreach (var tempQna in ask.Element("qnas").Elements("qna"))
            {
                var questionText = tempQna.Element("q").Element(natCode).Value;
                
                var answerList = new List<string>();
                var codeList = new List<string>();

                foreach (var a in tempQna.Element("as").Elements("a"))
                {
                    answerList.Add(a.Element(natCode).Value);
                    codeList.Add(a.Attribute("code")?.Value);
                }
                
                tempQnas.Add(new questionData
                {
                    title = tempQTitles[tempQnas.Count],
                    question = questionText,
                    answers = answerList,
                    codes = codeList
                });
            }
            // ===========

            // 16개 직업
            var tempJobs = new List<jobData>();

            for(int i = 1; i <= 16; i++)
            {
                XElement job = result.Element($"job{i}");

                tempJobs.Add(new jobData
                {
                    man = job.Element("man").Element(natCode).Value,
                    woman = job.Element("woman").Element(natCode).Value,
                    keywordMan = job.Element("keyword").Element("man").Element(natCode).Value,
                    keywordWoman = job.Element("keyword").Element("woman").Element(natCode).Value,
                    summaryMan = job.Element("summary").Element("man").Element(natCode).Value,
                    summaryWoman = job.Element("summary").Element("woman").Element(natCode).Value,
                    skill = job.Element("skill").Element(natCode).Value,
                    attitude = job.Element("attitude").Element(natCode).Value,
                    code = job.Element("code").Value
                });
            }
            // ==========

            lang = new LanguageConfig
            {
                // 공통
                next = general.Element("next").Element(natCode).Value,
                back = general.Element("back").Element(natCode).Value,
                home = general.Element("home").Element(natCode).Value,

                // 질문
                ready = ask.Element("ready").Element(natCode).Value,
                gender = ask.Element("gender").Element(natCode).Value,
                man = ask.Element("man").Element(natCode).Value,
                woman = ask.Element("woman").Element(natCode).Value,
                other = ask.Element("other").Element(natCode).Value,

                qnas = tempQnas,

                // 카메라
                camReady1 = cam.Element("camReady1").Element(natCode).Value,
                camReady2 = cam.Element("camReady2").Element(natCode).Value,
                lookUp = cam.Element("lookUp").Element(natCode).Value,
                recapture = cam.Element("recapture").Element(natCode).Value,
                capture = cam.Element("capture").Element(natCode).Value,

                // 로딩
                drawing = loading.Element("drawing").Element(natCode).Value,

                // 결과
                resultTitle = result.Element("title").Element(natCode).Value,
                qrSave = result.Element("qrSave").Element(natCode).Value,
                exit = result.Element("exit").Element(natCode).Value,
                qrTitle = result.Element("qrTitle").Element(natCode).Value,
                jobs = tempJobs,

                // 에러
                invalidSelection = error.Element("invalidSelection").Element(natCode).Value,
                aiPcCommunicationFailed = error.Element("aiPcCommunicationFailed").Element(natCode).Value,
                nlogInitFailed = error.Element("nlogInitFailed").Element(natCode).Value,
                dataFileLoadFailed = error.Element("dataFileLoadFailed").Element(natCode).Value,
                communicationFailed = error.Element("communicationFailed").Element(natCode).Value,
                programInitFailed = error.Element("programInitFailed").Element(natCode).Value,
                faceRecognitionFailed = error.Element("faceRecognitionFailed").Element(natCode).Value,
                aiPcError = error.Element("aiPcError").Element(natCode).Value
            };
        }
        catch (Exception ex)
        {
            NLogManager.Error($"LANGUAGE xml init error: {ex}");

            throw;
        }
    }
}