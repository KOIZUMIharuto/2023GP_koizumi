using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class player_status : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public string Name;
	public int Greade;
	public GameObject textPrefab; // インスペクタから TextMeshPro プレハブをアサイン
    private TextMeshProUGUI nameText;
	public GameObject textObject;

	private bool isHovered = false;
    private float hoverTime = 0f;
    public float hoverThreshold = 1f; // ホバーのしきい値（秒）

    private bool isDragging = false;


	public class Player_scores
	{
		public int Base_operation { get; set; }
		public int Replace_shime { get; set; }
		public int Move_shime { get; set; }
		public int Replace_miya { get; set; }
		public int Replace_oodaiko { get; set; }
		public int Move_oodaiko { get; set; }

		public Player_scores()
		{
			Base_operation = 0;
			Replace_shime = 0;
			Move_shime = 0;
			Replace_miya = 0;
			Replace_oodaiko = 0;
			Move_oodaiko = 0;
		}
	}

	public Player_scores Scores = new Player_scores();

	public List<Vector3> player_position = new List<Vector3>();
	public Vector3 initial_position;
	bool can_drag;
	int pre_now_number = -1;
	int now_number;
    // Start is called before the first frame update
    void Start()
    {
		initial_position = this.transform.position;

        Canvas canvas = FindObjectOfType<Canvas>();

		textObject = Instantiate(textPrefab, transform.position, Quaternion.identity);
        textObject.transform.SetParent(canvas.transform);

        // TextMeshPro オブジェクトに名前を設定
        nameText = textObject.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = Name;
        }
    }

    // Update is called once per frame
    void Update()
    {	
		GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
		can_drag = GameObject.Find("main_system").GetComponent<main_script>().can_drag;
		now_number = GameObject.Find("main_system").GetComponent<main_script>().now_number;
		if (pre_now_number != now_number)
		{
			if (now_number >= player_position.Count)
			{
				player_position.Add(initial_position);
			}
			if (pre_now_number >= 0)
			{
				player_position[pre_now_number] = this.transform.position;
			}
			this.transform.position = player_position[now_number];
			pre_now_number = now_number;
		}
		if (textObject != null)
        {
            Vector3 objectPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, this.transform.position);
            textObject.transform.position = objectPosition;
        }

		if (!isDragging && isHovered)
        {
            hoverTime += Time.deltaTime;
            if (hoverTime >= hoverThreshold)
            {
                DisplayStatus();
                // isHovered = false; // ステータスを表示したらホバーを無効にする
            }
        }
    }
	void OnMouseDrag()
    {
		now_number = GameObject.Find("main_system").GetComponent<main_script>().now_number;
		if (can_drag)
		{
			Vector3 thisPosition = Input.mousePosition;
			Vector3 worldPosition = Camera.main.ScreenToWorldPoint(thisPosition);
			worldPosition.z = 0f;
			this.transform.position = worldPosition;
		}
    }

	public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHover();
    }

    void ResetHover()
    {
        isHovered = false;
		// set color black
		GetComponent<Renderer>().material.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        hoverTime = 0f;
    }

    void DisplayStatus()
    {
		// set color red
		GetComponent<Renderer>().material.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        Debug.Log("Status Displayed");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        ResetHover(); // ドラッグ中にホバーを無効にする
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }
}
