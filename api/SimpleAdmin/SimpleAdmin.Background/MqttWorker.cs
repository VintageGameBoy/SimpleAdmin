

using Masuit.Tools;
using NewLife.MQTT;
using SimpleAdmin.Core;

namespace SimpleAdmin.Background;

/// <summary>
/// mqtt��̨����
/// </summary>
public class MqttWorker : BackgroundService
{
    private readonly ILogger<MqttWorker> _logger;
    private readonly ISimpleRedis _simpleRedis;
    private readonly MqttClient _mqtt;

    public MqttWorker(ILogger<MqttWorker> logger, ISimpleRedis simpleRedis, IMqttClientManager mqttClientManager)
    {
        _logger = logger;
        this._simpleRedis = simpleRedis;
        this._mqtt = mqttClientManager.GetClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //�����豸����������
        await _mqtt.SubscribeAsync("$SYS/brokers/+/clients/+/+", (e) =>
        {
            var topicList = e.Topic.Split("/");//����/�ָ�
            var clientId = topicList[topicList.Length - 2];//��ȡ�ͻ���ID
            if (clientId.Contains("_"))//�жϿͻ���ID�Ƿ����»������»��߱�ʾ��web�û���¼
            {
                var userId = clientId.Split("_")[0];
                //��ȡredis��ǰ�û���token��Ϣ�б�
                var tokenInfos = _simpleRedis.HashGetOne<List<TokenInfo>>(RedisConst.Redis_UserToken, userId);
                if (tokenInfos != null)
                {
                    var connectEvent = topicList.Last();//��ȡ�����¼��ж����߻�������
                    if (connectEvent == "connected")//���������
                    {
                        _logger.LogInformation($"�豸{clientId}������");
                        var token = _simpleRedis.Get<string>(RedisConst.Redis_MqttClientUser + clientId);//��ȡmqtt�ͻ���ID��Ӧ���û�token
                        if (token == null) return;//û��token��ֱ���˳�
                        //��ȡredis�е�ǰtoken
                        var tokenInfo = tokenInfos.Where(it => it.Token == token).FirstOrDefault();
                        if (tokenInfo != null)
                        {
                            tokenInfo.ClientIds.Add(clientId);//��ӵ��ͻ����б�
                            _simpleRedis.HashAdd(RedisConst.Redis_UserToken, userId, tokenInfos);//����Redis
                        }

                    }
                    else //����
                    {
                        _logger.LogInformation($"�豸{clientId}������");
                        //��ȡ��ǰ�ͻ���ID���ڵ�token��Ϣ
                        var tokenInfo = tokenInfos.Where(it => it.ClientIds.Contains(clientId)).FirstOrDefault();
                        if (tokenInfo != null)
                        {
                            tokenInfo.ClientIds.RemoveWhere(it => it == clientId);//�ӿͻ����б�ɾ��
                            _simpleRedis.HashAdd(RedisConst.Redis_UserToken, userId, tokenInfos);//����Redis
                        }
                    }
                }
            }
        });
    }
}