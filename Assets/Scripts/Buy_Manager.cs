using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;

public class Buy_Manager : MonoBehaviour, IStoreListener
{
    public Play_Script play_script;
    public Button no_ADS;

    static IStoreController m_StoreController;
    static IExtensionProvider m_StoreExtensionProvider;

    private void Start()
    {
        //���� ������� ��������� - ������ �� �������
        if (PlayerPrefs.GetInt("ADS") == 0)
            no_ADS.interactable = false;
    }

    public void Buy_no_ADS()
    {
        BuyProductID("no ADS");
    }

    void BuyProductID(string productID)
    {
        Product product = m_StoreController.products.WithID(productID);

        if (product != null && product.availableToPurchase)
        {
            Debug.Log("����� ���");
            m_StoreController.InitiatePurchase(product);
        }
        else
            Debug.Log("�� ������� ������");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (String.Equals(args.purchasedProduct.definition.id, "no ADS", StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

            //�������� ��� �������
            Debug.Log("���������� �������");
            no_ADS.interactable = false;
            PlayerPrefs.SetInt("ADS", 0);
            play_script.ADS_off();
        }
        else
            Debug.Log("������ � ��������");

        return PurchaseProcessingResult.Complete;
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        throw new NotImplementedException();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        throw new NotImplementedException();
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        throw new NotImplementedException();
    }
}
