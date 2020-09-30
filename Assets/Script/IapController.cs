 
# define DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuaweiConstants;
using HuaweiMobileServices.Base;
using HuaweiMobileServices.IAP;
using System;
using UnityEngine.Events;
using HuaweiMobileServices.Id;
using HmsPlugin;

public class IapController : MonoBehaviour
{

    public string[] ConsumableProducts;
    public string[] NonConsumableProducts;
    public string[] SubscriptionProducts;

    [HideInInspector]
    public int numberOfProductsRetrieved;


    List<ProductInfo> productInfoList = new List<ProductInfo>();
    List<string> productPurchasedList = new List<string>();

    private IapManager iapManager;
    private static  AccountManager accountManager;
    private static int temp = 0;
    UnityEvent loadedEvent;
    void Awake()
    {
        Debug.Log("[HMSPlugin]: IAPP manager Init");
        loadedEvent = new UnityEvent();
    }

    // Start is called before the first frame update
    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        Debug.Log($"[HMSPlugin]: IAP checkxxxx");
        iapManager = GetComponent<IapManager>();
        iapManager.OnCheckIapAvailabilitySuccess = LoadStore;
        iapManager.OnCheckIapAvailabilityFailure = (error) =>
        {
            Debug.Log($"[HMSPlugin]: IAP check failed. {error.Message}");
        };
        iapManager.CheckIapAvailability();
        Debug.Log($"[HMSPlugin]: IAP checkxxxx2");
    }

    private void SignedIn(AuthHuaweiId authHuaweiId)
    {
        Debug.Log("[HMS]: SignedIn: " + authHuaweiId + ":");
        iapManager = GetComponent<IapManager>();
        iapManager.OnCheckIapAvailabilitySuccess = LoadStore;
        iapManager.OnCheckIapAvailabilityFailure = (error) =>
        {
            Debug.Log($"[HMSPlugin]: IAP check failed. {error.Message}");
        };
        iapManager.CheckIapAvailability();
    }

    private void LoadStore()
    {
        Debug.Log("[HMS]: LoadStorexxx");
        // Set Callback for ObtainInfoSuccess
        iapManager.OnObtainProductInfoSuccess = (productInfoResultList) =>
        {
            Debug.Log("[HMS]: LoadStore1");
            if (productInfoResultList != null)
            {
                Debug.Log("[HMS]: LoadStore2");
                foreach (ProductInfoResult productInfoResult in productInfoResultList)
                {
                    foreach (ProductInfo productInfo in productInfoResult.ProductInfoList)
                    {
                        productInfoList.Add(productInfo);
                        Debug.Log("[HMS]: productInfoList: " + productInfo.ProductName + " : " + productInfo.PriceType);
                    }

                }
            }
            loadedEvent.Invoke();

        };
        // Set Callback for ObtainInfoFailure
        iapManager.OnObtainProductInfoFailure = (error) =>
        {
            Debug.Log($"[HMSPlugin]: IAP ObtainProductInfo failed. {error.Message},,, {error.WrappedExceptionMessage},,, {error.WrappedCauseMessage}");
        };

        // Call ObtainProductInfo 
       if (!IsNullOrEmpty(ConsumableProducts))
        {
           iapManager.ObtainProductConsumablesInfo(new List<string>(ConsumableProducts));
        }
       if (!IsNullOrEmpty(NonConsumableProducts))
        {
            iapManager.ObtainProductNonConsumablesInfo(new List<string>(NonConsumableProducts));
        }
        if (!IsNullOrEmpty(SubscriptionProducts))
        {
            iapManager.ObtainProductSubscriptionInfo(new List<string>(SubscriptionProducts));
        } 

    }

    private void RestorePurchases()
    {
        iapManager.OnObtainOwnedPurchasesSuccess = (ownedPurchaseResult) =>
        {
            productPurchasedList = (List<string>)ownedPurchaseResult.InAppPurchaseDataList;
        };

        iapManager.OnObtainOwnedPurchasesFailure = (error) =>
        {
            Debug.Log("[HMS:] RestorePurchasesError" + error.Message);
        };

        iapManager.ObtainOwnedPurchases();
    }

    public ProductInfo GetProductInfo(string productID)
    {
        return productInfoList.Find(productInfo => productInfo.ProductId == productID);
    }

    public void showHidePanelDynamically(GameObject yourObject)
    {
        Debug.Log("[HMS:] showHidePanelDynamically");

        var getCanvasGroup = yourObject.GetComponent<CanvasGroup>();
        if (getCanvasGroup.alpha == 0)
        {
            getCanvasGroup.alpha = 1;
            getCanvasGroup.interactable = true;

        }
        else
        {
            getCanvasGroup.alpha = 0;
            getCanvasGroup.interactable = false;
        }

    }

    public void BuyProduct(string productID)
    {
        iapManager.OnBuyProductSuccess = (purchaseResultInfo) =>
        {
            // Verify signature with purchaseResultInfo.InAppDataSignature

            // If signature ok, deliver product

            // Consume product purchaseResultInfo.InAppDataSignature
            iapManager.ConsumePurchase(purchaseResultInfo);

        };

        iapManager.OnBuyProductFailure = (errorCode) =>
        {

            switch (errorCode)
            {
                case OrderStatusCode.ORDER_STATE_CANCEL:
                    // User cancel payment.
                    Debug.Log("[HMS]: User cancel payment");
                    break;
                case OrderStatusCode.ORDER_STATE_FAILED:
                    Debug.Log("[HMS]: order payment failed");
                    break;

                case OrderStatusCode.ORDER_PRODUCT_OWNED:
                    Debug.Log("[HMS]: Product owned");
                    break;
                default:
                    Debug.Log("[HMS:] BuyProduct ERROR" + errorCode);
                    break;
            }
        };

        var productInfo = productInfoList.Find(info => info.ProductId == productID);
        var payload = "test";

        iapManager.BuyProduct(productInfo, payload);

    }


    public void addListener(UnityAction action)
    {
        if (loadedEvent != null)
        {
            loadedEvent.AddListener(action);
        }

    }
    public bool IsNullOrEmpty(Array array)
    {
        return (array == null || array.Length == 0);
    }

} 
