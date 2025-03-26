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
        newMessage.Content = newText;
        newMessage.Role = "user";
        messages.Add(newMessage);

        newMessage.Role = "system";
        newMessage.Content = "당신은 아주 재치있고 웃긴 총게임 어시스턴트야. 존댓말을 쓰고.";
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
