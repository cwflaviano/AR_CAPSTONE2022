using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinectStreams : MonoBehaviour
{
    [SerializeField] private KinectConfig KinectConfig;

    // returns the raw depth/user data, if ComputeUserMap is true
    public ushort[] GetRawDepthMap()
    {
        return KinectConfig.usersDepthMap;
    }

    // returns the depth data for a specific pixel, if ComputeUserMap is true
    public ushort GetDepthForPixel(int x, int y)
    {
        int index = y * KinectWrapper.Constants.DepthImageWidth + x;

        if (index >= 0 && index < KinectConfig.usersDepthMap.Length)
            return KinectConfig.usersDepthMap[index];
        else
            return 0;
    }

    // returns the depth map position for a 3d joint position
    public Vector2 GetDepthMapPosForJointPos(Vector3 posJoint)
    {
        Vector3 vDepthPos = KinectWrapper.MapSkeletonPointToDepthPoint(posJoint);
        Vector2 vMapPos = new Vector2(vDepthPos.x, vDepthPos.y);

        return vMapPos;
    }

    // returns the color map position for a depth 2d position
    public Vector2 GetColorMapPosForDepthPos(Vector2 posDepth)
    {
        int cx, cy;

        KinectWrapper.NuiImageViewArea pcViewArea = new KinectWrapper.NuiImageViewArea
        {
            eDigitalZoom = 0,
            lCenterX = 0,
            lCenterY = 0
        };

        KinectWrapper.NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(
            KinectWrapper.Constants.ColorImageResolution,
            KinectWrapper.Constants.DepthImageResolution,
            ref pcViewArea,
            (int)posDepth.x, (int)posDepth.y, GetDepthForPixel((int)posDepth.x, (int)posDepth.y),
            out cx, out cy);

        return new Vector2(cx, cy);
    }

    // returns the depth image/users histogram texture,if ComputeUserMap is true
    public Texture2D GetUsersLblTex()
    {
        return KinectConfig.usersLblTex;
    }

    // returns the color image texture,if ComputeColorMap is true
    public Texture2D GetUsersClrTex()
    {
        return KinectConfig.usersClrTex;
    }

    // use user map
    public void UseUserMap()
    {
        if (KinectConfig.ComputeUserMap)
            if (KinectConfig.depthStreamHandle != IntPtr.Zero && KinectWrapper.PollDepth(KinectConfig.depthStreamHandle, KinectWrapper.Constants.IsNearMode, ref KinectConfig.usersDepthMap))
                UpdateUserMap();
    }

    // Use Color map - for displaying camera feed
    public void UseColorMap()
    {
        if (KinectConfig.ComputeColorMap)
            if (KinectConfig.colorStreamHandle != IntPtr.Zero && KinectWrapper.PollColor(KinectConfig.colorStreamHandle, ref KinectConfig.usersColorMap, ref KinectConfig.colorImage))
                UpdateColorMap();
    }



    // Update the User Map
    private void UpdateUserMap()
    {
        int numOfPoints = 0;
        Array.Clear(KinectConfig.usersHistogramMap, 0, KinectConfig.usersHistogramMap.Length);

        // Calculate cumulative histogram for depth
        for (int i = 0; i < KinectConfig.usersMapSize; i++)
        {
            // Only calculate for depth that contains users
            if ((KinectConfig.usersDepthMap[i] & 7) != 0)
            {
                ushort userDepth = (ushort)(KinectConfig.usersDepthMap[i] >> 3);
                KinectConfig.usersHistogramMap[userDepth]++;
                numOfPoints++;
            }
        }

        if (numOfPoints > 0)
        {
            for (int i = 1; i < KinectConfig.usersHistogramMap.Length; i++)
            {
                KinectConfig.usersHistogramMap[i] += KinectConfig.usersHistogramMap[i - 1];
            }

            for (int i = 0; i < KinectConfig.usersHistogramMap.Length; i++)
            {
                KinectConfig.usersHistogramMap[i] = 1.0f - (KinectConfig.usersHistogramMap[i] / numOfPoints);
            }
        }

        // dummy structure needed by the coordinate mapper
        KinectWrapper.NuiImageViewArea pcViewArea = new KinectWrapper.NuiImageViewArea
        {
            eDigitalZoom = 0,
            lCenterX = 0,
            lCenterY = 0
        };

        // Create the actual users texture based on label map and depth histogram
        Color32 clrClear = Color.clear;
        for (int i = 0; i < KinectConfig.usersMapSize; i++)
        {
            // Flip the texture as we convert label map to color array
            int flipIndex = i; // usersMapSize - i - 1;

            ushort userMap = (ushort)(KinectConfig.usersDepthMap[i] & 7);
            ushort userDepth = (ushort)(KinectConfig.usersDepthMap[i] >> 3);

            ushort nowUserPixel = userMap != 0 ? (ushort)((userMap << 13) | userDepth) : userDepth;
            ushort wasUserPixel = KinectConfig.usersPrevState[flipIndex];

            // draw only the changed pixels
            if (nowUserPixel != wasUserPixel)
            {
                KinectConfig.usersPrevState[flipIndex] = nowUserPixel;

                if (userMap == 0)
                {
                    KinectConfig.usersMapColors[flipIndex] = clrClear;
                }
                else
                {
                    if (KinectConfig.colorImage != null)
                    {
                        int x = i % KinectWrapper.Constants.DepthImageWidth;
                        int y = i / KinectWrapper.Constants.DepthImageWidth;

                        int cx, cy;
                        int hr = KinectWrapper.NuiImageGetColorPixelCoordinatesFromDepthPixelAtResolution(
                            KinectWrapper.Constants.ColorImageResolution,
                            KinectWrapper.Constants.DepthImageResolution,
                            ref pcViewArea,
                            x, y, KinectConfig.usersDepthMap[i],
                            out cx, out cy);

                        if (hr == 0)
                        {
                            int colorIndex = cx + cy * KinectWrapper.Constants.ColorImageWidth;
                            //colorIndex = usersMapSize - colorIndex - 1;
                            if (colorIndex >= 0 && colorIndex < KinectConfig.usersMapSize)
                            {
                                Color32 colorPixel = KinectConfig.colorImage[colorIndex];
                                KinectConfig.usersMapColors[flipIndex] = colorPixel;  // new Color(colorPixel.r / 256f, colorPixel.g / 256f, colorPixel.b / 256f, 0.9f);
                                KinectConfig.usersMapColors[flipIndex].a = 230; // 0.9f
                            }
                        }
                    }
                    else
                    {
                        // Create a blending color based on the depth histogram
                        float histDepth = KinectConfig.usersHistogramMap[userDepth];
                        Color c = new Color(histDepth, histDepth, histDepth, 0.9f);

                        switch (userMap % 4)
                        {
                            case 0:
                                KinectConfig.usersMapColors[flipIndex] = Color.red * c;
                                break;
                            case 1:
                                KinectConfig.usersMapColors[flipIndex] = Color.green * c;
                                break;
                            case 2:
                                KinectConfig.usersMapColors[flipIndex] = Color.blue * c;
                                break;
                            case 3:
                                KinectConfig.usersMapColors[flipIndex] = Color.magenta * c;
                                break;
                        }
                    }
                }

            }
        }

        // Draw it!
        KinectConfig.usersLblTex.SetPixels32(KinectConfig.usersMapColors);

        if (!KinectConfig.DisplaySkeletonLines)
        {
            KinectConfig.usersLblTex.Apply();
        }
    }

    // Update the Color Map
    private void UpdateColorMap()
    {
        KinectConfig.usersClrTex.SetPixels32(KinectConfig.colorImage);
        KinectConfig.usersClrTex.Apply();
    }



    // draws the skeleton in the given texture
    public void DrawSkeleton(Texture2D aTexture, ref KinectWrapper.NuiSkeletonData skeletonData, ref bool[] playerJointsTracked)
    {
        int jointsCount = (int)KinectWrapper.NuiSkeletonPositionIndex.Count;

        for (int i = 0; i < jointsCount; i++)
        {
            int parent = KinectWrapper.GetSkeletonJointParent(i);

            if (playerJointsTracked[i] && playerJointsTracked[parent])
            {
                Vector3 posParent = KinectWrapper.MapSkeletonPointToDepthPoint(skeletonData.SkeletonPositions[parent]);
                Vector3 posJoint = KinectWrapper.MapSkeletonPointToDepthPoint(skeletonData.SkeletonPositions[i]);

                //				posParent.y = KinectWrapper.Constants.ImageHeight - posParent.y - 1;
                //				posJoint.y = KinectWrapper.Constants.ImageHeight - posJoint.y - 1;
                //				posParent.x = KinectWrapper.Constants.ImageWidth - posParent.x - 1;
                //				posJoint.x = KinectWrapper.Constants.ImageWidth - posJoint.x - 1;

                //Color lineColor = playerJointsTracked[i] && playerJointsTracked[parent] ? Color.red : Color.yellow;
                DrawLine(aTexture, (int)posParent.x, (int)posParent.y, (int)posJoint.x, (int)posJoint.y, Color.yellow);
            }
        }
    }

    // draws a line in a texture
    private void DrawLine(Texture2D a_Texture, int x1, int y1, int x2, int y2, Color a_Color)
    {
        int width = a_Texture.width;  // KinectWrapper.Constants.DepthImageWidth;
        int height = a_Texture.height;  // KinectWrapper.Constants.DepthImageHeight;

        int dy = y2 - y1;
        int dx = x2 - x1;

        int stepy = 1;
        if (dy < 0)
        {
            dy = -dy;
            stepy = -1;
        }

        int stepx = 1;
        if (dx < 0)
        {
            dx = -dx;
            stepx = -1;
        }

        dy <<= 1;
        dx <<= 1;

        if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    a_Texture.SetPixel(x1 + x, y1 + y, a_Color);

        if (dx > dy)
        {
            int fraction = dy - (dx >> 1);

            while (x1 != x2)
            {
                if (fraction >= 0)
                {
                    y1 += stepy;
                    fraction -= dx;
                }

                x1 += stepx;
                fraction += dy;

                if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                            a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
            }
        }
        else
        {
            int fraction = dx - (dy >> 1);

            while (y1 != y2)
            {
                if (fraction >= 0)
                {
                    x1 += stepx;
                    fraction -= dy;
                }

                y1 += stepy;
                fraction += dx;

                if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                            a_Texture.SetPixel(x1 + x, y1 + y, a_Color);
            }
        }

    }
}
