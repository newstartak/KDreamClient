using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class WebRtcPythonReceiver : Singleton<WebRtcPythonReceiver>
{
    [Serializable]
    class SdpRequest
    {
        public string type;
        public string sdp;
    }

    [Serializable]
    class SdpResponse
    {
        public string type;
        public string sdp;
    }

    private string offerPostUrl;

    [SerializeField] public RawImage receiveView;
    [SerializeField] private List<RawImage> receiveViews = new List<RawImage>();

    RTCPeerConnection pc;
    VideoStreamTrack recvTrack;
    Texture latestVideoTexture;

    protected override void Init()
    {
        StartCoroutine(WebRTC.Update());
        StartCoroutine(StartFlow());
    }

    public void RegisterReceiveView(RawImage view)
    {
        if (view == null)
            return;

        if (!receiveViews.Contains(view))
            receiveViews.Add(view);

        if (latestVideoTexture != null)
            view.texture = latestVideoTexture;
    }

    public void UnregisterReceiveView(RawImage view)
    {
        if (view == null)
            return;

        receiveViews.Remove(view);
    }

    public void ClearReceiveViews()
    {
        receiveViews.Clear();
    }

    IEnumerator StartFlow()
    {
        offerPostUrl = XmlManager.camera.webRtcUrl;

        pc = new RTCPeerConnection();

        pc.OnTrack = e =>
        {
            if (e.Track is VideoStreamTrack vt)
            {
                recvTrack = vt;
                recvTrack.OnVideoReceived += OnVideoReceived;
                NLogManager.Info("OnTrack video");
            }
        };

        pc.OnConnectionStateChange = state =>
        {
            NLogManager.Info("ConnectionState: " + state);
        };

        pc.OnIceConnectionChange = state =>
        {
            NLogManager.Info("IceConnectionState: " + state);
        };

        pc.OnIceGatheringStateChange = state =>
        {
            NLogManager.Info("GatheringState: " + state);
        };

        pc.AddTransceiver(TrackKind.Video).Direction = RTCRtpTransceiverDirection.RecvOnly;

        var offerOp = pc.CreateOffer();
        yield return offerOp;

        if (offerOp.IsError)
        {
            NLogManager.Error("CreateOffer error: " + offerOp.Error.message);
            yield break;
        }

        var offer = offerOp.Desc;

        var setLocalOp = pc.SetLocalDescription(ref offer);
        yield return setLocalOp;

        if (setLocalOp.IsError)
        {
            NLogManager.Error("SetLocalDescription error: " + setLocalOp.Error.message);
            yield break;
        }

        while (pc.GatheringState != RTCIceGatheringState.Complete)
            yield return null;

        string offerSdp = pc.LocalDescription.sdp;
        NLogManager.Info("Offer SDP length: " + (offerSdp != null ? offerSdp.Length : 0));

        yield return StartCoroutine(PostOfferAndApplyAnswer(offerSdp));
    }

    IEnumerator PostOfferAndApplyAnswer(string offerSdp)
    {
        var reqObj = new SdpRequest
        {
            type = "offer",
            sdp = offerSdp
        };

        string requestJson = JsonUtility.ToJson(reqObj);

        using (var req = new UnityWebRequest(offerPostUrl, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(requestJson);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                NLogManager.Error("HTTP error: " + req.error);
                if (req.downloadHandler != null)
                    NLogManager.Error("HTTP body: " + req.downloadHandler.text);
                yield break;
            }

            string responseRaw = req.downloadHandler.text;
            NLogManager.Info("HTTP response raw:\n" + responseRaw);

            SdpResponse resp;
            try
            {
                resp = JsonUtility.FromJson<SdpResponse>(responseRaw);
            }
            catch
            {
                NLogManager.Error("Response is not valid JSON");
                yield break;
            }

            if (resp == null)
            {
                NLogManager.Error("Response JSON parse returned null");
                yield break;
            }

            if (resp.type != "answer")
            {
                NLogManager.Warn("Response type is not 'answer': " + resp.type);
            }

            if (string.IsNullOrEmpty(resp.sdp))
            {
                NLogManager.Error("Response JSON has empty sdp");
                yield break;
            }

            NLogManager.Info("Answer SDP length: " + resp.sdp.Length);

            var desc = new RTCSessionDescription
            {
                type = RTCSdpType.Answer,
                sdp = resp.sdp
            };

            var op = pc.SetRemoteDescription(ref desc);
            yield return op;

            if (op.IsError)
            {
                NLogManager.Error("SetRemoteDescription error: " + op.Error.message);
                NLogManager.Error("Answer SDP was:\n" + resp.sdp);
                yield break;
            }

            NLogManager.Info("Applied answer");
        }
    }

    void OnVideoReceived(Texture tex)
    {
        latestVideoTexture = tex;

        if (receiveView != null)
            receiveView.texture = tex;

        for (int i = 0; i < receiveViews.Count; i++)
        {
            RawImage view = receiveViews[i];
            if (view != null)
                view.texture = tex;
        }
    }

    void OnDestroy()
    {
        if (recvTrack != null)
            recvTrack.OnVideoReceived -= OnVideoReceived;

        if (pc != null)
        {
            pc.Close();
            pc.Dispose();
            pc = null;
        }
    }
}
