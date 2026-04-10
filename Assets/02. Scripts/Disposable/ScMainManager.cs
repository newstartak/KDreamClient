using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScMainManager : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button _ko;
    [SerializeField] private Button _en;
    [SerializeField] private Button _zh;
    [SerializeField] private Button _ja;

    private List<Button> _btns;


    [Header("배경음 및 효과음")]
    [SerializeField] private List<AudioClip> _clips;


    [Header("폰트")]
    [SerializeField] private TMP_FontAsset _koFont;
    [SerializeField] private TMP_FontAsset _jaFont;
    [SerializeField] private TMP_FontAsset _zhFont;



    [Header("테스트용")]
    [SerializeField] private Button _trBtn;
    [SerializeField] private Button _tlBtn;
    [SerializeField] private Button _brBtn;
    [SerializeField] private TMP_Text _testText;


    private int _topRightCount = 0;
    private int _topLeftCount = 0;
    private int _bottomRightCount = 0;

    async void Awake()
    {
        ProgramManager.Instance.sounds = _clips;

        
        bool initOk = await ProgramManager.Instance.EnsureInitializedAsync();
        if (!initOk)
        {
            await SceneWorker.ChangeSceneAsync("scError");
            return;
        }

        InitScene();

        InitBtns();

        RedisWorker.PublishSimpleRedis("INIT", "");



        // 테스트용
        InitTestBtns();
    }

    void InitScene()
    {
        // 테스트용
        if(ProgramManager.Instance.isTestMode)
        {
            _testText.text = "TEST MODE\n(No Cam, Redis Btn, Use Local Img)";
        }
    }

    void InitBtns()
    {
        _btns = new List<Button> { _ko, _en, _zh, _ja };

        _ko.onClick.AddListener(() => SetLanguage(NationCode.ko));
        _en.onClick.AddListener(() => SetLanguage(NationCode.en));
        _zh.onClick.AddListener(() => SetLanguage(NationCode.zh));
        _ja.onClick.AddListener(() => SetLanguage(NationCode.ja));

        foreach(var btn in _btns)
        {
            btn.onClick.AddListener(() => SceneWorker.ChangeScene("scAsk"));
        }
    }

    void SetLanguage(NationCode nc)
    {
        XmlManager.SetLangXml(nc);

        _koFont.fallbackFontAssetTable.Clear();

        switch(nc)
        {
            case NationCode.ko:
                RedisWorker.PublishSimpleRedis("ko", "", "lang", "");
                break;

            case NationCode.ja:
                RedisWorker.PublishSimpleRedis("ja", "", "lang", "");
                _koFont.fallbackFontAssetTable.Add(_jaFont);
                break;

            case NationCode.zh:
                RedisWorker.PublishSimpleRedis("zh", "", "lang", "");
                _koFont.fallbackFontAssetTable.Add(_zhFont);
                break;

            case NationCode.en:
                RedisWorker.PublishSimpleRedis("en", "", "lang", "");
                break;
        }

        NLogManager.Info($"NationCode Changed: {nc}");
    }





    // 테스트용
    void InitTestBtns()
    {
        _trBtn.onClick.AddListener(() => SetBtns(ref _topRightCount, 10, -1, -1, () => NLogManager.Debug("test mode unlocking 1/3")));
        _tlBtn.onClick.AddListener(() => SetBtns(ref _topLeftCount, 4, _topRightCount, 10, () => NLogManager.Debug("test mode unlocking 2/3")));
        
        _brBtn.onClick.AddListener(() => SetBtns(ref _bottomRightCount, 6, _topLeftCount, 4, () => 
        {
            _topRightCount = 0;
            _topLeftCount = 0;
            _bottomRightCount = 0;
            NLogManager.Debug("test mode unlocked 3/3");

            ProgramManager.Instance.isTestMode = !ProgramManager.Instance.isTestMode;

            Cursor.visible = ProgramManager.Instance.isTestMode;

            NLogManager.Debug($"Test Mode: {ProgramManager.Instance.isTestMode}");

            if(ProgramManager.Instance.isTestMode)
            {
                _testText.text = "TEST MODE\n(No Cam, Redis Btn, Use Local Img)";
            }
            else
            {
                _testText.text = "";
            }
        }));
    }

    /// <param name="clickedBtnCount">현재 클릭하여 카운트가 1 올라가야 하는 카운트 변수</param>
    /// <param name="curTargetCount">현재 클릭하여 카운트가 1 올라가야 하는 카운트 변수가 달성해야 하는 수</param>
    /// <param name="prevClickedBtnCount">해당 버튼을 누르기 전에 누르고 와야 하는 버튼의 카운트 변수, 없을 경우 -1로 기재</param>
    /// <param name="prevTargetCount">해당 버튼을 누르기 전에 누르과 와야 하는 버튼의 카운트 변수가 달성했어야 하는 수, 없을 경우 -1로 기재</param>
    /// <param name="prevTargetCount">해당 버튼 목표 카운트만큼 누르기 성공 시 호출할 메서드, 없을 경우 미기재</param>
    void SetBtns(ref int clickedBtnCount, int curTargetCount, int prevClickedBtnCount, int prevTargetCount, Action method = null)
    {
        clickedBtnCount++;

        if(clickedBtnCount == curTargetCount && prevClickedBtnCount == prevTargetCount)
        {
            method();
        }
        else if(clickedBtnCount > curTargetCount || prevClickedBtnCount != prevTargetCount)
        {
            _topRightCount = 0;
            _topLeftCount = 0;
            _bottomRightCount = 0;
            NLogManager.Debug("test mode unlock failed");
        }
    }
}
