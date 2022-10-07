using UnityEngine;
using Amazon;
using Amazon.S3;
using Amazon.CognitoIdentity;
using Amazon.S3.Model;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO;
using Siccity.GLTFUtility;
using System.Collections;

public class AWSManager : MonoBehaviour
{
    public byte[] data = null;
    public bool ObjectDownloaded = false;
    #region Singleton
    private static AWSManager _instance;
    public static AWSManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("AWSManager is Null");
            }
            return _instance;
        }
    }
    #endregion

    public string S3Region = RegionEndpoint.APSouth1.SystemName;

    private RegionEndpoint _S3Region
    {
        get { return RegionEndpoint.GetBySystemName(S3Region); }
    }

    private AmazonS3Client _s3Client;

    public AmazonS3Client S3Client
    {
        get
        {
            if (_s3Client == null)
            {
                _s3Client = new AmazonS3Client(new CognitoAWSCredentials("ap-south-1:4e44c5c9-c0ca-4f08-b656-fb943ee4ae5e", RegionEndpoint.APSouth1), _S3Region);
            }
            return _s3Client;
        }
    }

    private void Awake()
    {
        _instance = this;
        UnityInitializer.AttachToGameObject(this.gameObject);
        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
        S3Client.ListBucketsAsync(new ListBucketsRequest(), (responseObject) =>
         {
             if (responseObject.Exception == null)
             {
                 responseObject.Response.Buckets.ForEach((s3b) => 
                 {
                     if (s3b.BucketName == "aws-virtufit-app-bucket")
                     {
                         Debug.Log($"{s3b.BucketName} Acess is available");
                     }
                 });
             }
             else
             {
                 Debug.Log($"AWS Error {responseObject.Exception.HResult}");
             }
 
         });
    }

    #region Model Loading
    public void GetS3Object(string AvatarID, Action onComplete = null)
    {
        string target = $"{AvatarID}";
        ObjectDownloaded = false;

        var request = new ListObjectsRequest()
        {
            BucketName = "aws-virtufit-app-bucket"
        };

        S3Client.ListObjectsAsync(request, (responseObject) =>
        {
            if (responseObject.Exception == null)
            {
                bool AvatarFound = responseObject.Response.S3Objects.Any(obj => obj.Key == target);
                if (AvatarFound)
                {
                    Debug.Log("Download Started");
                   StartCoroutine(Download(onComplete, target, AvatarID));
                }
                else
                {
                    Debug.Log("Error 404, Object Not Found");
                }
            }
            else
            {
                Debug.Log("Listing Error From S3" + responseObject.Exception);
            }
        });
    }

    IEnumerator Download(Action OnComplete, string target, string AvatarID)
    {
        S3Client.GetObjectAsync("aws-virtufit-app-bucket", target, (responseObject) =>
        {
            if (responseObject.Exception == null)
            {
                if (responseObject.Response.ResponseStream != null)
                {
                    data = null;

                    using (StreamReader reader = new StreamReader(responseObject.Response.ResponseStream))
                    {
                        using (MemoryStream memory = new MemoryStream())
                        {
                            var buffer = new byte[512];
                            var bytesRead = default(int);

                            while ((bytesRead = reader.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                memory.Write(buffer, 0, bytesRead);
                            }
                            data = memory.ToArray();
                            Debug.Log($"File Size is {data.Length/1024/1024} MB");
                            Debug.Log("Download Completed");
                        }
                    }
                    if (OnComplete != null) OnComplete();
                            ObjectDownloaded = true;
                }
            }
        });
        yield return null;
    }
    #endregion
}
