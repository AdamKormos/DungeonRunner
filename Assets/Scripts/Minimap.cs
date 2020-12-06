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
    Image[][] images = default;
    bool[][] visitedRooms = default;
    int prevI = -1, prevJ = -1;

    // Start is called before the first frame update
    void Start()
    {
        images = new Image[MapManager.s_gridSize.y][];
        visitedRooms = new bool[MapManager.s_gridSize.y][];

        containerBottomLeftPosition = new Vector3(-imageContainerTransform.rect.width / 2f, imageContainerTransform.rect.width / 2f);
        roomImageOffsets = new Vector2(5f + sampleRoomImage.GetComponent<RectTransform>().rect.width, 5f + sampleRoomImage.GetComponent<RectTransform>().rect.height);
        CreateMinimap();
    }

    private void Update()
    {
        if(Player.currentRoomI != prevI || Player.currentRoomJ != prevJ)
        {
            OnRoomChanged();
        }

        prevI = Player.currentRoomI;
        prevJ = Player.currentRoomJ;
    }

    private void OnRoomChanged()
    {
        int offsetI = Player.currentRoomI - prevI, offsetJ = Player.currentRoomJ - prevJ;
        if(prevI != -1 && prevJ != -1) images[prevI][prevJ].color = Color.white;
        //imageContainerTransform.transform.localPosition += new Vector3(jAmount * -55f, iAmount * 70f);
        imageContainerTransform.offsetMin += new Vector2(offsetJ * -55f, -offsetI * 35f);
        imageContainerTransform.offsetMax += new Vector2(offsetJ * -55f, -offsetI * 35f);

        images[prevI + offsetI][prevJ + offsetJ].color = Color.blue;
        images[prevI + offsetI][prevJ + offsetJ].gameObject.SetActive(true);
        visitedRooms[prevI + offsetI][prevJ + offsetJ] = true;
    }

    private void CreateMinimap()
    {
        for(int i = 0; i < MapManager.s_gridSize.y; i++)
        {
            images[i] = new Image[MapManager.s_gridSize.x];
            visitedRooms[i] = new bool[MapManager.s_gridSize.x];
            for(int j = 0; j < MapManager.s_gridSize.x; j++)
            {
                GameObject roomImage = Instantiate(sampleRoomImage, sampleRoomImage.transform.position, Quaternion.identity, imageContainerTransform.transform);
                roomImage.transform.localPosition = new Vector2(j * roomImageOffsets.x, i * roomImageOffsets.y) - containerBottomLeftPosition;
                images[i][j] = roomImage.GetComponent<Image>();
                roomImage.SetActive(false);
            }
        }

        sampleRoomImage.SetActive(false);
    }
}
