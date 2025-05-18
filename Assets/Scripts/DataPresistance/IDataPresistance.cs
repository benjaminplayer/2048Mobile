using UnityEngine;

public interface IDataPresistance
{
    void LoadData(GameData data);

    void SaveData(ref GameData data); // ref = reference

}
