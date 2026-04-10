using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ProgramManager : Singleton<ProgramManager>
{
    public string errorMsg;
    public bool isTestMode = false;

    private bool _isInitialized = false;
    private Task<bool> _initTask;

    private AudioSource _bgmSrc;
    private AudioSource _sfxSrc;

    public List<AudioClip> sounds;

    public void PlaySound(int index)
    {
        _sfxSrc.generator = sounds[index];
        _sfxSrc.Play();
    }

    public async Task<bool> EnsureInitializedAsync()
    {
        if (_isInitialized)
        {
            return true;
        }

        if (_initTask != null && !_initTask.IsCompleted)
        {
            return await _initTask;
        }

        _initTask = InitProgramInternalAsync();
        return await _initTask;
    }

    private async Task<bool> InitProgramInternalAsync()
    {
        try
        {
            await NLogManager.InitNLog();
        }
        catch
        {
            errorMsg = "NLog Init Error. Please contact the administrator.";
            return false;
        }

        try
        {
            await XmlManager.InitXml();
        }
        catch
        {
            NLogManager.Error("Xml Init Error");
            errorMsg = XmlManager.lang.dataFileLoadFailed;
            return false;
        }

        try
        {
            await RedisWorker.InitRedis();
        }
        catch
        {
            NLogManager.Error("Redis Init Error");
            errorMsg = XmlManager.lang.communicationFailed;
            return false;
        }

        try
        {
            InitProgram();
            await UploadManager.InitAsync();
        }
        catch
        {
            NLogManager.Error("Program Init Error");
            errorMsg = XmlManager.lang.programInitFailed;
            return false;
        }

        _isInitialized = true;

        // 배경음 및 효과음 설정
        _bgmSrc = gameObject.AddComponent<AudioSource>();
        _sfxSrc = gameObject.AddComponent<AudioSource>();
        _bgmSrc.generator = sounds[0];
        _bgmSrc.loop = true;
        _bgmSrc.volume = 0.75f;
        _bgmSrc.Play();


        await Task.Delay(3000);
        WindowSetting.AssignTopmostWindow("", true, true);

        return true;
    }

    public void InitProgram()
    {
        NLogManager.Info("ProgramManager init completed");
    }

    void Update()
    {
        if (RedisWorker.commands.TryDequeue(out string dequeuedCmd))
        {
            ScAskManager manager = ScAskManager.Instance;

            switch (dequeuedCmd)
            {
                case "face_on":
                    if (manager != null)
                    {
                        if (manager.canCapture)
                        {
                            StartCoroutine(manager.Capture());
                        }
                    }
                    break;

                case "face_off":
                    if (manager != null)
                    {

                    }
                    break;


                case "ai_start":
                    NLogManager.Info("AI START execute");
                    StartCoroutine(ScAskManager.Instance.InitLoadingSet()); 
                    break;

                case "ai_complete":
                    NLogManager.Info("ai complete execute");
                    ScAskManager.Instance.InitResultSet();
                    break;

                case "capture_fail":
                    StartCoroutine(DelayCapture());


                    break;

                case "capture_error":
                    ProgramManager.Instance.errorMsg = XmlManager.lang.faceRecognitionFailed;
                    SceneWorker.ChangeScene("scError");
                    break;
            }
        }
    }

    IEnumerator DelayCapture()
    {
        ScAskManager.Instance.PrepareReCapture();

        yield return new WaitForSeconds(3);

        if(ScAskManager.Instance != null)
        {
            ScAskManager.Instance.canCapture = true;
        }
    }
}