using UnityEngine;


[CreateAssetMenu(fileName = "PlayerDataScriptableObject", menuName = "ScriptableObjects/PlayerData")]
public class PlayerDataScriptableObject : ScriptableObject
{
    public string user_id;
    public string user_password;
    public string user_name;
    public string user_email;
}
