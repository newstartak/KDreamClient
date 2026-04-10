using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScErrorManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _errorText;

    [SerializeField] private Button _backBtn;
    [SerializeField] private TMP_Text _backText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _errorText.text = ProgramManager.Instance.errorMsg;
        _backText.text = XmlManager.lang.back;

        _backBtn.onClick.AddListener(() => SceneWorker.ChangeScene("scMain"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
