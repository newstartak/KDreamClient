using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum curState
{
    gender,
    question
}

public class ScAskManager : MonoBehaviour
{
    [Header("UI 세트 모음")]
    [SerializeField] private GameObject _generalSet;
    [Space]
    [SerializeField] private GameObject _readySet;
    [Space]
    [SerializeField] private GameObject _genderSet;
    [Space]
    [SerializeField] private GameObject _ask1ans3Set;
    [Space]
    [SerializeField] private GameObject _ask1ans4Set;
    [Space]
    [SerializeField] private GameObject _camSet;
    [Space]
    [SerializeField] private GameObject _loadingSet;
    [Space]
    [SerializeField] private GameObject _resultSet;

    [Header("general 세트")]
    [SerializeField] private Button _backBtn;
    [SerializeField] private TMP_Text _backText;
    [SerializeField] private RawImage _background;
    [SerializeField] private Texture2D _redBg;
    [Space]
    
    [SerializeField] private Button _homeBtn;
    [SerializeField] private TMP_Text _homeText;

    [Header("ready 세트")]
    [SerializeField] private TMP_Text _readyTxt;

    [Header("gender 세트")]
    [SerializeField] private TMP_Text _genderText;
    [Space]
    [SerializeField] private Button _womanBtn;
    [SerializeField] private TMP_Text _womanText;
    [Space]
    [SerializeField] private Button _manBtn;
    [SerializeField] private TMP_Text _manText;
    [Space]
    [SerializeField] private Button _otherBtn;
    [SerializeField] private TMP_Text _otherText;

    [Header("질문 3 개짜리 질문 세트")]
    [SerializeField] private List<TMP_Text> _ans3questionTexts;
    [Space]
    [SerializeField] private List<TMP_Text> _ans3answerTexts;
    [Space]
    [SerializeField] private List<Button> _ans3answerBtns;

    [Header("질문 4 개짜리 질문 세트")]
    [SerializeField] private List<TMP_Text> _ans4questionTexts;
    [Space]
    [SerializeField] private List<TMP_Text> _ans4answerTexts;
    [Space]
    [SerializeField] private List<Button> _ans4answerBtns;

    [Header("cam 세트")]
    [SerializeField] private GameObject _cameraReady;
    [SerializeField] private TMP_Text _cameraReadyText;
    [SerializeField] private TMP_Text _lookUpText;
    [SerializeField] private Button _cameraReadyBtn;
    [SerializeField] private TMP_Text _cameraReadyBtnText;
    [Space]
    [SerializeField] private GameObject _realCamera;
    [SerializeField] private RawImage _webRtcCam;
    [SerializeField] private GameObject _faceDetection;
    [SerializeField] private TMP_Text _cameraCount;
    [Space]
    [SerializeField] private GameObject _cameraBlur;

    [Header("로딩 세트")]
    [SerializeField] private TMP_Text _loadingText;

    [Header("result 세트")]
    [SerializeField] private RawImage _resultImg;
    [Space]
    [SerializeField] private List<TMP_Text> _resultTitleTexts;
    [SerializeField] private List<TMP_Text> _resultDescTexts;
    [Space]
    [SerializeField] private Button _qrOpenBtn;
    [SerializeField] private TMP_Text _qrOpenText;
    [Space]
    [SerializeField] private Button _exitBtn;
    [SerializeField] private TMP_Text _exitText;
    [Space]
    [SerializeField] private GameObject _qrframe;
    [SerializeField] private TMP_Text _qrTitleText;
    [SerializeField] private RawImage _qrImg;
    [SerializeField] private Button _qrExitBtn;
    [SerializeField] private TMP_Text _qrExitBtnText;

    [Header("fonts")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TMP_FontAsset _koFont;
    [SerializeField] private TMP_FontAsset _jaFont;
    [SerializeField] private TMP_FontAsset _zhFont;

    private Stack<string> _code;
    private int _curQuestionNum;

    private curState _curState;

    public bool canCapture;

    private string _pickedJob;

    public static ScAskManager Instance {get; private set;}

    [SerializeField] private List<GameObject> _resultDelayUIs;

    private Coroutine _returnCo; 

    [Header("테스트용")]
    [SerializeField] private Button _aiStartBtn;
    [SerializeField] private Button _aiCompleteBtn;

    void Awake()
    {
        InitScene();
        InitCoroutine(XmlManager.program.askWait);
        InitGeneralSet();

        // 시작점
        DisableReadySet();

        // 테스트용
        InitTestBtns();
    }

    void InitScene()
    {
        Instance = this;

        _code = new Stack<string>();

        _curQuestionNum = 0;
        canCapture = false;

        _pickedJob = "";

        if(ProgramManager.Instance.isTestMode == false)
        {
            WebRtcPythonReceiver.Instance.RegisterReceiveView(_webRtcCam);
        }


        var allTexts = _canvas.GetComponentsInChildren<TMP_Text>(true);

        foreach (var text in allTexts)
        {
            switch (LanguageManager.GetNationCode())
            {
                case "ko":
                case "en":
                    text.font = _koFont; 
                    break;
                
                case "ja":
                    text.font = _jaFont;
                    break;

                case "zh":
                    text.font = _zhFont;
                    break;
            }
        }
    }

    void InitCoroutine(int waitTime)
    {
        if(_returnCo != null)
        {
            StopCoroutine(_returnCo);
            _returnCo = null;
        }
        _returnCo = StartCoroutine(ReturnToMain(waitTime));
    }

    IEnumerator ReturnToMain(int waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        NLogManager.Debug("Timeout. Return to main scene");
        SceneWorker.ChangeScene("scMain");
    }

    void InitGeneralSet()
    {
        _backText.text = XmlManager.lang.back;
        _homeText.text = XmlManager.lang.home;

        void ClickBackBtn()
        {
            switch(_curState)
            {
                case curState.gender:
                    ClickHomeBtn();
                    break;

                case curState.question:
                    ProgramManager.Instance.PlaySound(1);

                    if(_curQuestionNum <= 0)
                    {
                        InitGenderSet();
                    }
                    else
                    {
                        _curQuestionNum--;
                        InitAskSet();
                    }
                    break;
            }

            _code.TryPop(out var popCode);
            NLogManager.Info($"{popCode} has been canceled.");
        }
        _backBtn.onClick.RemoveAllListeners();
        _backBtn.onClick.AddListener(() => 
        {
            InitCoroutine(XmlManager.program.askWait);
            ClickBackBtn();
        });

        void ClickHomeBtn()
        {
            SceneWorker.ChangeScene("scMain");
        }
        _homeBtn.onClick.RemoveAllListeners();  
        _homeBtn.onClick.AddListener(() => ClickHomeBtn());
    }

    void DisableReadySet()
    {
        _readySet.SetActive(true);

        _readyTxt.text = XmlManager.lang.ready;

        Invoke(nameof(DisableReadySetAfter), 2f);
    }

    void DisableReadySetAfter()
    {
        _readySet.SetActive(false);

        InitGenderSet();
    }

    void InitGenderSet()
    {
        _curState = curState.gender;

        _ask1ans3Set.SetActive(false);
        _ask1ans4Set.SetActive(false);

        _genderSet.SetActive(true);

        _genderText.text = XmlManager.lang.gender;
        _womanText.text = XmlManager.lang.woman;
        _manText.text = XmlManager.lang.man;
        _otherText.text = XmlManager.lang.other;

        void ClickGenderBtn(string g)
        {
            ProgramManager.Instance.PlaySound(1);

            InitCoroutine(XmlManager.program.askWait);

            if(string.IsNullOrEmpty(g))
            {
                int rand = UnityEngine.Random.Range(0, 1);

                switch(rand)
                {
                    case 0:
                        g = "M";
                        break;

                    case 1:
                        g = "W";
                        break;
                }
            }

           _code.Push(g);

            NLogManager.Info($"Gender Selected: {g}");

            InitAskSet();
        }

        _womanBtn.onClick.RemoveAllListeners();
        _manBtn.onClick.RemoveAllListeners();
        _otherBtn.onClick.RemoveAllListeners();

        _womanBtn.onClick.AddListener(() => ClickGenderBtn("W"));
        _manBtn.onClick.AddListener(() => ClickGenderBtn("M"));
        _otherBtn.onClick.AddListener(() => ClickGenderBtn(""));
    }

    void InitAskSet()
    {
        _curState = curState.question;

        _genderSet.SetActive(false);
        _ask1ans3Set.SetActive(false);
        _ask1ans4Set.SetActive(false);

        var curQuestion = XmlManager.lang.qnas[_curQuestionNum];

        void ClickAnsBtn(string ansCode)
        {
            ProgramManager.Instance.PlaySound(1);

            InitCoroutine(XmlManager.program.askWait);

            _code.Push(ansCode);

            NLogManager.Info($"code added: {ansCode}");

            // 질문이 남아있는 경우
            if(_curQuestionNum < XmlManager.lang.qnas.Count - 1)
            {
                _curQuestionNum++;
                InitAskSet();
            }
            // 마지막 질문이었을 경우
            else
            {
                InitCameraSet();
            }
        }

        if(curQuestion.answers.Count == 3)
        {
            _ask1ans3Set.SetActive(true);

            _ans3questionTexts[0].text = curQuestion.title;
            _ans3questionTexts[1].text = curQuestion.question;

            for(int i = 0; i < curQuestion.answers.Count; i++)
            {
                _ans3answerTexts[i].text = curQuestion.answers[i];
                _ans3answerBtns[i].onClick.RemoveAllListeners();

                var code = curQuestion.codes[i];
                _ans3answerBtns[i].onClick.AddListener(() => ClickAnsBtn(code));
            }
        }
        else
        {
            _ask1ans4Set.SetActive(true);

            _ans4questionTexts[0].text = curQuestion.title;
            _ans4questionTexts[1].text = curQuestion.question;

            for(int i = 0; i < curQuestion.answers.Count; i++)
            {
                _ans4answerTexts[i].text = curQuestion.answers[i];
                _ans4answerBtns[i].onClick.RemoveAllListeners();

                var code = curQuestion.codes[i];
                _ans4answerBtns[i].onClick.AddListener(() => ClickAnsBtn(code));
            }
        }
    }

    void InitCameraSet()
    {
        _generalSet.SetActive(false);

        _ask1ans3Set.SetActive(false);
        _ask1ans4Set.SetActive(false);

        _camSet.SetActive(true);

        _cameraReadyText.text = XmlManager.lang.camReady1;

        _background.texture = _redBg;

        StartCoroutine(ChangeCameraSet1());
    }

    IEnumerator ChangeCameraSet1()
    {
        yield return new WaitForSeconds(7);

        _cameraReadyText.text = XmlManager.lang.camReady2;

        StartCoroutine(ChangeCameraSet2());
    }

    IEnumerator ChangeCameraSet2()
    {
        yield return new WaitForSeconds(5);

        _cameraReadyText.text = string.Empty;

        _realCamera.SetActive(true);
        _cameraReadyBtn.gameObject.SetActive(true);

        _cameraReadyBtnText.text = XmlManager.lang.capture;
        _lookUpText.text = XmlManager.lang.lookUp;

        _cameraReadyBtn.onClick.AddListener(() =>
        {
            ProgramManager.Instance.PlaySound(1);

            InitCoroutine(XmlManager.program.askWait);

            _cameraReady.SetActive(false);
            RedisWorker.isCameraActive = true;
            canCapture = true;
        });
    }

    public void PrepareReCapture()
    {
        _lookUpText.text = XmlManager.lang.recapture;
    }

    public IEnumerator Capture()
    {
        _lookUpText.text = XmlManager.lang.lookUp;

        canCapture = false;

        for(int i = XmlManager.program.captureWait; i > 0; i--)
        {
            _cameraCount.text = i.ToString();

            yield return new WaitForSeconds(1);
        }

        _cameraCount.text = "";
        ProgramManager.Instance.PlaySound(3);
        
        var tempCode = _code;
        for (int i = 3; i > 0; i--)
        {             
            while(tempCode.Count > 0)
            {
                _pickedJob = tempCode.Pop() + _pickedJob;
            }

            if (_pickedJob.Length == 5 &&
                (_pickedJob.ToLower().StartsWith('m') || _pickedJob.ToLower().StartsWith('w')))
            {
                NLogManager.Info($"valid code: {_pickedJob}");
                break;   
            }

            if(i <= 1)
            {
                NLogManager.Error($"invalid code: {_pickedJob}");
                ProgramManager.Instance.errorMsg = XmlManager.lang.invalidSelection;
                SceneWorker.ChangeScene("scError");
                yield break;
            }
        }

        NLogManager.Info($"Determined job code is {_pickedJob}");

        RedisWorker.PublishSimpleRedis("CAPTURE_PHOTO", _pickedJob);
    }

    public IEnumerator InitLoadingSet()
    {
        RedisWorker.isCameraActive = false;

        InitCoroutine(XmlManager.program.askWait);

        _cameraBlur.SetActive(true);

        yield return new WaitForSeconds(2);

        _camSet.SetActive(false);
        _loadingSet.SetActive(true);

        _loadingText.text = XmlManager.lang.drawing;
    }

    IEnumerator DelayShowResultUI()
    {
        InitCoroutine(XmlManager.program.resultWait);

        yield return new WaitForSeconds(3);

        foreach(var uiGo in _resultDelayUIs)
        {
            uiGo.SetActive(true);
        }
    }

    public void InitResultSet()
    {
        _loadingSet.SetActive(false);
        _resultSet.SetActive(true);

        StartCoroutine(DelayShowResultUI());
        
        _resultTitleTexts[0].text = XmlManager.lang.resultTitle;

        int jobIndex;
        switch (RedisWorker.finalJob)
        {
            case "임금":
            case "중전":
                jobIndex = 0;
                break;

            case "거상":
                jobIndex = 1;
                break;

            case "영의정":
            case "상궁":
                jobIndex = 2;
                break;

            case "전기수":
            case "책비":
                jobIndex = 3;
                break;

            case "암행어사":
            case "다모":
                jobIndex = 4;
                break;

            case "성균관유생":
                jobIndex = 5;
                break;

            case "어의":
            case "의녀":
                jobIndex = 6;
                break;

            case "국수":
                jobIndex = 7;
                break;

            case "명창":
                jobIndex = 8;
                break;

            case "도공":
            case "침선장":
                jobIndex = 9;
                break;

            case "숙수":
            case "수랏간궁녀":
                jobIndex = 10;
                break;

            case "도화서화원":
            case "수모":
                jobIndex = 11;
                break;

            case "장군":
                jobIndex = 12;
                break;

            case "포작":
            case "잠녀":
                jobIndex = 13;
                break;

            case "멸화군":
                jobIndex = 14;
                break;

            case "착호갑사":
                jobIndex = 15;
                break;

            default:
                jobIndex = -1;
                NLogManager.Error($"Invalid job name received: {RedisWorker.finalJob}");
            
                ProgramManager.Instance.errorMsg = XmlManager.lang.aiPcCommunicationFailed;
                SceneWorker.ChangeScene("scError");
                break;
        }

        bool isWoman;
        switch (RedisWorker.finalJob)
        {
            case "중전":
            case "상궁":
            case "책비":
            case "다모":
            case "의녀":
            case "수모":
            case "침선장":
            case "수랏간궁녀":
            case "잠녀":
                isWoman = true;
                break;

            default:
                isWoman = false;
                break;
        }

        if(isWoman)
        {
            _resultTitleTexts[1].text = isWoman ? XmlManager.lang.jobs[jobIndex].woman : XmlManager.lang.jobs[jobIndex].woman;

            _resultDescTexts[0].text = isWoman ? XmlManager.lang.jobs[jobIndex].summaryWoman : XmlManager.lang.jobs[jobIndex].summaryWoman;
            _resultDescTexts[1].text = isWoman ? XmlManager.lang.jobs[jobIndex].keywordWoman : XmlManager.lang.jobs[jobIndex].keywordWoman;
        }
        else
        {
            _resultTitleTexts[1].text = isWoman ? XmlManager.lang.jobs[jobIndex].woman : XmlManager.lang.jobs[jobIndex].man;

            _resultDescTexts[0].text = isWoman ? XmlManager.lang.jobs[jobIndex].summaryWoman : XmlManager.lang.jobs[jobIndex].summaryMan;
            _resultDescTexts[1].text = isWoman ? XmlManager.lang.jobs[jobIndex].keywordWoman : XmlManager.lang.jobs[jobIndex].keywordMan;
        }


        // 이미지 불러오기
        byte[] resultImgBytes = File.ReadAllBytes(RedisWorker.finalJobPath);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(resultImgBytes);

        _resultImg.texture = tex;
        // =============

        _qrOpenBtn.onClick.AddListener(() => 
        {
            ProgramManager.Instance.PlaySound(1);

            InitCoroutine(XmlManager.program.resultWait);
            _qrframe.SetActive(true);
        });
        _qrOpenText.text = XmlManager.lang.qrSave;

        _exitBtn.onClick.AddListener(() => SceneWorker.ChangeScene("scMain"));
        _exitText.text = XmlManager.lang.exit;

        _qrTitleText.text = XmlManager.lang.qrTitle;

        _qrExitBtn.onClick.AddListener(() => SceneWorker.ChangeScene("scMain"));
        _qrExitBtnText.text = XmlManager.lang.exit;

        var resultImageName = $"{DateTime.Now:MMdd}{Guid.NewGuid().ToString("N").Substring(0, 10)}";

        UploadManager.UploadMemoryAsync(resultImgBytes, resultImageName, "image/png");
        _qrImg.texture = UploadManager.GenerateQrCodeTexture($"https://xdream-7b7c5.web.app/?file={resultImageName}&lang={LanguageManager.GetNationCode()}");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
















    // 테스트용
    void InitTestBtns()
    {
        _aiStartBtn.gameObject.SetActive(false);
        _aiCompleteBtn.gameObject.SetActive(false);

        if(ProgramManager.Instance.isTestMode)
        {    
            _aiStartBtn.gameObject.SetActive(true);
            _aiCompleteBtn.gameObject.SetActive(true);

            _aiStartBtn.onClick.AddListener(() => RedisWorker.PublishSimpleRedis("AI_START", ""));


            Dictionary<string, object> jsonObj = new Dictionary<string, object>
            {
                ["path"] = @"C:\xorbis\Company\Project\K-Dream\result\Step3_PNG\final_image.png",
                ["job"] = "임금"
            };
            _aiCompleteBtn.onClick.AddListener(() => RedisWorker.PublishRedisAsync("AI_COMPLETE", jsonObj));
        }

    }
}