using StackExchange.Redis;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

public static class RedisWorker
{
    private static ConnectionMultiplexer _redis;

    private static ISubscriber _sub;

    public static ConcurrentQueue<string> commands;
    public static string finalJob;
    public static string finalJobPath;

    public static bool isCameraActive = false;

    public static async Task InitRedis()
    {
        commands = new ConcurrentQueue<string>();

        bool isInitFailed = true;

        foreach (var ip in XmlManager.redis.endPoint)
        {
            var config = new ConfigurationOptions
            {
                AbortOnConnectFail = true,
                ConnectTimeout = 2000,
                EndPoints = { $"{ip}:{XmlManager.redis.port}" }
            };

            if (string.IsNullOrEmpty(XmlManager.redis.password) == false)
            {
                config.Password = XmlManager.redis.password;
            }

            try
            {
                _redis = await ConnectionMultiplexer.ConnectAsync(config);

                NLogManager.Info($"Redis server connected: {ip}:{XmlManager.redis.port}");

                _sub = _redis.GetSubscriber();
                _sub.Subscribe(RedisChannel.Literal(XmlManager.redis.channel), async (ch, cmd) =>
                {
                    await HandleCommand(cmd);
                });

                _redis.ConnectionFailed += (_, ex) =>
                {
                    NLogManager.Warn($"Redis disconnected: {ex.Exception}");

                    _redis = null;
                    _sub = null;
                };

                isInitFailed = false;
                break;
            }
            catch (Exception ex)
            {
                NLogManager.Error($"Error occured during connecting to redis server: {ex.Message}");
            }
        }

        if(isInitFailed)
        {
            throw new Exception("No Alive Redis Server");
        }
    }

    private static async Task HandleCommand(string cmd)
    {
        JObject jsonObj = JObject.Parse(cmd);

        string step = jsonObj["step"]?.ToString().ToLower();

        string path = "";
        string job = "";

        string errorMsg = "";

        if(jsonObj["data"] is JObject dataObj)
        {
            path = dataObj["path"]?.ToString().ToLower();
            job = dataObj["job"]?.ToString().ToLower();
        }
        else
        {
            errorMsg = jsonObj["data"]?.ToString().ToLower();
        }


        string key = jsonObj["key"]?.ToString().ToLower();
        string value = jsonObj["value"]?.ToString().ToLower();

        switch (step)
        {
            case "ai_start":
                NLogManager.Info("redis get AI START");
                
                commands.Enqueue(step);
                break;

            case "ai_complete":
                NLogManager.Info("redis get AI complete");
                finalJobPath = path;
                finalJob = job;

                commands.Enqueue(step);
                break;

            case "error":
                NLogManager.Error($"Error Occured: {errorMsg}");
                ProgramManager.Instance.errorMsg = XmlManager.lang.aiPcError;
                SceneWorker.ChangeScene("scError");

                commands.Enqueue(step);
                break;

            case "capture_fail":
                NLogManager.Warn($"capture fail: {cmd}");

                commands.Enqueue(step);
                break;

            case "capture_error":
                NLogManager.Warn($"capture error: {cmd}");

                commands.Enqueue(step);
                break;
        }

        switch (key)
        {
            case "face_detect":
                if(int.Parse(value) == 1 && isCameraActive)
                {
                    commands.Enqueue("face_on");
                }
                else if(int.Parse(value) == 0 && isCameraActive)
                {
                    commands.Enqueue("face_off");
                }

                break;
        }


    }

    public static async Task PublishRedisAsync(string step, Dictionary<string, object> data)
    {
        JObject jsonObj = new JObject
        {
            ["step"] = step,
            ["data"] = JObject.FromObject(data)
        };

        string json = jsonObj.ToString();

        try
        {
            await _sub.PublishAsync(RedisChannel.Literal(XmlManager.redis.channel), json);
        }
        catch(Exception ex)
        {
            NLogManager.Error($"Error occured during sending cmd: {ex.Message}");

            throw;
        }

        NLogManager.Info($"Command sent: {json}");
    }

        public static async Task PublishSimpleRedis(string stepValue, string dataValue, string stepKey = "step", string dataKey = "data")
    {
        JObject jsonObj = new JObject
        {
            [stepKey] = stepValue,
            [dataKey] = dataValue
        };

        if(dataKey == "")
        {
            jsonObj.Remove(dataKey);
        }

        string json = jsonObj.ToString();

        try
        {
            await _sub.PublishAsync(RedisChannel.Literal(XmlManager.redis.channel), json);
        }
        catch(Exception ex)
        {
            NLogManager.Error($"Error occured during sending cmd: {ex.Message}");

            throw;
        }

        NLogManager.Info($"Command sent: {json}");
    }
}