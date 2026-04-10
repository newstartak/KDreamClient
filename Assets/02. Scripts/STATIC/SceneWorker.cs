using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public static class SceneWorker
{
    public static void ChangeScene(string scName)
    {
        ProgramManager.Instance.PlaySound(2);
        SceneManager.LoadScene(scName);
    }

    public static async Task ChangeSceneAsync(string scName)
    {
        await SceneManager.LoadSceneAsync(scName);
    }
}