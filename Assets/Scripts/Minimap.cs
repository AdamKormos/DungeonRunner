using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class Minimap : MonoBehaviour
{
    [SerializeField] RectTransform imageContainerTransform = default;
    [SerializeField] GameObject sampleRoomImage = default;
    Vector2 containerBottomLeftPosition = default;
    Vector2 roomImageOffsets = default;
    List<List<Image>> images = new List<List<Image>>();

    // Start is called before the first frame update
    void Start()
    {
        containerBottomLeftPosition = new Vector3(-imageContainerTransform.rect.width / 2f, -imageContainerTransform.rect.width / 2f);
        roomImageOffsets = new Vector2(5f + sampleRoomImage.GetComponent<RectTransform>().rect.width, 5f + sampleRoomImage.GetComponent<RectTransform>().rect.height);
        CreateMinimap();
    }

    private void Update()
    {
        imageContainerTransform.anchoredPosition =
                new Vector2(Player.currentRoomJ * -roomImageOffsets.x, Player.currentRoomI * -roomImageOffsets.y);
        images[Player.currentRoomI][Player.currentRoomJ].color = Color.blue;
    }

    private void CreateMinimap()
    {
        for(int i = 0; i < MapManager.s_gridSize.x; i++)
        {
            images.Add(new List<Image>());
            for(int j = 0; j < MapManager.s_gridSize.y; j++)
            {
                if(MapManager.roomGrid[i][j] != null)
                {
                    GameObject roomImage = Instantiate(sampleRoomImage, sampleRoomImage.transform.position, Quaternion.identity, imageContainerTransform.transform);
                    roomImage.transform.localPosition = containerBottomLeftPosition + new Vector2(j * roomImageOffsets.x, i * roomImageOffsets.y);
                    images[i].Add(roomImage.GetComponent<Image>());
                }
            }
        }

        sampleRoomImage.SetActive(false);
    }
}
