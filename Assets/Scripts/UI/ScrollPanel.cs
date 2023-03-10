using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollPanel : MonoBehaviour
{
    /*                                                     AÇIKLAMA
     *       
     *       Uygulamaya GridLayoutGroup ve ContentSizeFitter componentleri ile default değerleri belirleyerek başlıyoruz böylelikle 
     *       content elemanlarımız belirlediğimiz düzende başlıyor. Ancak scroll başladığında bu componentleri pasif hale getiriyoruz
     *       çünkü GridLayoutGroup alt objelerin rectTransformuna erişip düzenlemeye engel oluyor ve boyutlandırmayı kendimiz yaptığımız 
     *       için ContentSizeFitter'a ihtiyacımız kalmıyor.
     *    
     */

    private bool componentBool;

    [SerializeField] private GameObject button1;
    [SerializeField] private GameObject button2;

    private ScrollRect scrollRect;
    private RectTransform rectTransform;
    private RectTransform[,] rectTransforms;
    private RectOffset gridLayoutPadding;

    private Vector2 curButtonSize;

    private float height;
    private float offsetY;
    private float marginY;
    private int rowCounter;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        scrollRect = GetComponent<ScrollRect>();
        height = rectTransform.rect.height;
        scrollRect.onValueChanged.AddListener(OnScroll);
        gridLayoutPadding = scrollRect.content.GetComponent<GridLayoutGroup>().padding;

        updateCellSize();
        updateCellCount();
        updateCells();
        addButtons();
    }

    void updateCellSize()
    {
        //Ekran genişliğine göre ihtiyacımız olan kare buton boyutlarını belirliyoruz
        curButtonSize.x = (rectTransform.rect.width - gridLayoutPadding.left - gridLayoutPadding.right - scrollRect.content.GetComponent<GridLayoutGroup>().spacing.x) / 2;
        curButtonSize.y = curButtonSize.x;
    }
    void updateCellCount()
    {
        // GridLayoutGroup'ta belirlediğimiz padding ve y-space'leri yüksekliğe bölüp optimum satır sayısını buluyoruz
        rowCounter = Mathf.CeilToInt((height - (gridLayoutPadding.top + gridLayoutPadding.bottom)) / (curButtonSize.y + scrollRect.content.GetComponent<GridLayoutGroup>().spacing.y));

        // Satır sayısını boşluk kalma ihtimaline karşı 2 arttırıyoruz
        rowCounter = rowCounter + 2;
    }
    void updateCells()
    {
        //Belirlediğimiz hücre boyutlarını componente uyguluyoruz
        scrollRect.content.GetComponent<GridLayoutGroup>().cellSize = new Vector2(curButtonSize.x, curButtonSize.y);
    }
    void addButtons()
    {
        rectTransforms = new RectTransform[rowCounter, 2];

        //Bulduğumuz satır sayısı adedince 2 sütun olmak üzere butonlarımızı ekliyoruz
        //Bunu yaparken de daha sonra değiştirmek üzere butonların rectTransformlarını oluşturduğumuz diziye aktarıyoruz
        for (int i = 0; i < rowCounter; i++)
        {
            var button1Clone = Instantiate(button1, scrollRect.content);
            rectTransforms[i, 0] = button1Clone.GetComponent<RectTransform>();
            var button2Clone = Instantiate(button2, scrollRect.content);
            rectTransforms[i, 1] = button2Clone.GetComponent<RectTransform>();
        }
    }
    public void OnScroll(Vector2 pos)
    {
        if (!componentBool)
        {
            componentBool = true;
            
            //İki satır arası boşluğu hesaplıyoruz
            offsetY = rectTransforms[0, 0].position.y - rectTransforms[1, 0].position.y;
            
            //Bütün satırların Y düzlemindeki orjin noktasını buluyoruz
            marginY = offsetY * rowCounter / 2;
            scrollRect.content.GetComponent<ContentSizeFitter>().enabled = false;
            scrollRect.content.GetComponent<GridLayoutGroup>().enabled = false;
        }
        for (int i = 0; i < rowCounter; i++)
        {
            /*  Satırdaki ilk objenin parent objesine göreli uzaklığı, bulduğumuz Y eksen orjininden büyükse
            satırdaki iki objeyi de en alta taşıyoruz */
            if (scrollRect.transform.InverseTransformPoint(rectTransforms[i, 0].gameObject.transform.position).y > marginY)
            {
                for (int j = 0; j < 2; j++)
                {
                    rectTransforms[i, j].position = new Vector2(rectTransforms[i, j].position.x, rectTransforms[i, j].position.y - (marginY * 2));
                    scrollRect.content.GetChild(rowCounter * 2 - 1).transform.SetAsFirstSibling();
                }
            }
            /*  Satırdaki ilk objenin parent objesine göreli uzaklığı, bulduğumuz Y eksen orjininden küçükse
            satırdaki iki objeyi de en yukarı taşıyoruz */

            else if (scrollRect.transform.InverseTransformPoint(rectTransforms[i, 0].gameObject.transform.position).y < -marginY)
            {
                for (int j = 0; j < 2; j++)
                {
                    rectTransforms[i, j].position = new Vector2(rectTransforms[i, j].position.x, rectTransforms[i, j].position.y + (marginY * 2));
                    scrollRect.content.GetChild(0).transform.SetAsLastSibling();
                }
            }

        }
    }
}
