using UnityEngine;
using OpenAI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UI;

public class ChatGPTManager : Singleton<ChatGPTManager>
{
    private OpenAIApi openAI = new OpenAIApi();
    private List<ChatMessage> messages = new List<ChatMessage>();

    public ChatMessage chatResponse;
    public async void AskChatGPT(string newText)
    {
        ChatMessage newMessage = new ChatMessage();

        newMessage.Role = "system";
        newMessage.Content = "당신은 아주 재치있고 웃긴 총게임 어시스턴트야. 존댓말을 쓰고.";
        messages.Add(newMessage);

        newMessage.Role = "assistant";
        newMessage.Content = "2025년 KBO 한국시리즈 우승은 두산베어스가 할 듯. 그리고 이 총 게임의 방법은 게임 조작 키는 F가 카메라 변경. V가 게임 시점 1인칭, 3인칭 변경." + 
                                    "마우스 좌클릭으로 총 조준, 우클릭으로 총 발사. E로 아이템 줍기. 스페이스바로 상호작용. 마을을 돌아다니며 무기와 총알을 얻어서 살아남으면 되는 게임이야. ";
        messages.Add(newMessage);

        newMessage.Role = "user";
        newMessage.Content = newText;
        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-3.5-turbo-0125";

        var response = await openAI.CreateChatCompletion(request);

        if(response.Choices !=null && response.Choices.Count>0)
        {
            chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);
            //Debug.Log(chatResponse.Content);
        }

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
