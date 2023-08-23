using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    private UdpServer server;

    public TextMeshProUGUI txtLocIP;
    public TMP_InputField ifDstIP;
    public TMP_InputField ifDstPort;
    public TMP_InputField ifLocPort;
    public Button btnAdd;
    public Button btnClose;
    public TMP_InputField ifSend;
    public Button btnSend;
    public TMP_InputField ifReceive;

    // Start is called before the first frame update
    void Start()
    {

        Loom.Initialize();

        btnClose.gameObject.SetActive(false);

        txtLocIP.text = "本机IP:" + GetLocalIP();
        btnAdd.onClick.AddListener(() =>
        {
            BtnClick(btnAdd);
        });
        btnClose.onClick.AddListener(() =>
        {
            BtnClick(btnClose);
        });
        btnSend.onClick.AddListener(() =>
        {
            BtnClick(btnSend);
        });
        EventCenter.AddListener<string>("Receive", OnMessage);
        EventCenter.AddListener<string>("Error", OnError);
    }

    void BtnClick(Button button)
    {
        if (button.name == btnAdd.name)
        {
            if (!string.IsNullOrEmpty(ifDstIP.text)
                && !string.IsNullOrEmpty(ifDstPort.text)
                && !string.IsNullOrEmpty(ifLocPort.text))
            {
                server = new UdpServer();
                int locPort = 0;
                int.TryParse(ifLocPort.text, out locPort);
                int dstPort = 0;
                int.TryParse(ifDstPort.text, out dstPort);

                server.Start(locPort, ifDstIP.text, dstPort);

                ifDstIP.interactable = false;
                ifDstPort.interactable = false;
                ifLocPort.interactable = false;
            }
            btnAdd.gameObject.SetActive(false);
            btnClose.gameObject.SetActive(true);
        }
        if (button.name == btnClose.name)
        {
            server.Dispose();
            server = null;
            btnClose.gameObject.SetActive(false);
            btnAdd.gameObject.SetActive(true);

            ifDstIP.interactable = true;
            ifDstPort.interactable = true;
            ifLocPort.interactable = true;
        }
        if (button.name == btnSend.name)
        {
            if (server != null && !string.IsNullOrEmpty(ifSend.text))
            {
                server.Send(ifSend.text);
            }
        }
    }

    void OnMessage(string msg)
    {
        Loom.QueueOnMainThread(() =>
        {
            ifReceive.text += msg+ "\r\n";
        });
    }

    void OnError(string error)
    {
        Loom.QueueOnMainThread(() =>
        {
            ifReceive.text += error+ "\r\n";
        });
    }

    private void OnApplicationQuit()
    {
        EventCenter.RemoveListener<string>("Receive", OnMessage);
        EventCenter.RemoveListener<string>("Error", OnError);
    }

    /// <summary>
    /// 获取本机ip
    /// </summary>
    /// <returns></returns>
    string GetLocalIP()
    {
        string strHostName = Dns.GetHostName();
        IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
        IPAddress[] addr = ipEntry.AddressList;
        string ip = "";
        foreach (IPAddress ipd in addr)
        {
            if (ipd.ToString().Contains("."))
            {
                ip += "      " + ipd;
            }
        }
        return ip;
    }
}