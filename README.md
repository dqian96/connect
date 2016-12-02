# Connect

Connect translates American Sign Language (ASL) into English sentences. Connect can be broken down into several components/processes - they are listed below.

Disclaimer: Since this app was written by two people at a Hackathon, the code is messy and disorganized. However, a lot of thoughts were put into the actual algorithms used. <br/>

### Components:

* High Quality Video Capture (gather raw input data): 
The Microsoft Kinect is able to capture high quality, depth-sensitive video data.
This allows for easy hand/finger recongition.

* Finger/Hand Tracking (extracting relevant data): 
Although some signs in ASL use facial expressions as "part of the sign", signs generally utilize the hands and fingers exclusively. For this project, we only considered the hands and fingers of the person making the sign. An open source library was used to draw contours around the hands and fingers (a contour is a set of points outlining the hands/fingers).[1]
Each frame in the video was mapped to a set of 3D contour points.

* Delimiting Signs (semantic filtering/grouping of data): 
There are no specific delimiters used in ASL to separate one sign (word) from another.
For Connect, we made the assumption that hands slow down/stop at the start and end of a sign. We created an algorithm that is able to
consistently separate one word from another based on this assumption.[2]
Using this algorithm, contiguous frames were grouped together when they were believed to be part of one word/sign. 
Frames where no sign was detected were labelled as "empty frames" and pruned.

* Sign-to-word Prediction (interpreting data):
After the above process, we knew which frames belonged to which particular sign. From this sign-frames association, we created an ordered set for each individual sign. Each set contained 3D contour points, ordered based on the timestamp of the frame that the contour originated from. These 3D contour points were the features that we used for our machine learning. However, the number of elements were inconsistent across the sets. Therefore, for each frame, we only included a tenth of the contour points for each frame (about a third of the total points). In addition, we only used 20 frames from the entire duration of the sign. These frames were evenly distributed across the sign's duration. With the above modifications, each and every sign now had a consistent number of 3D points/features. Using Azure ML, we trained a multi-layer neural network using 60 video clips/training examples for 6 total signs; 20 of which were used for cross-validation. On average, the cross-validation data scored 98% on accuracy.
Unfortunately, my Azure ML subscription expired and the only picture I could salvage pertained to the accuracy of the NN on the cross-validation data.

* Performance Considerations (pruning unnecessary data): 
Recording and analyzing 3D video data was extremely taxing on our workstation. Even though we used only 20 frames and a tenth of the contour points for each sign, the performance of our application was still poor. Performance improvements are likely with a better computer.

* Next Steps:
Currently, the code is a mess since it was written at a hackathon. The code needs to be tidied up. There is a bug with the API call to Azure ML that needs to be fixed.

### Notes:

1. 
Code used for the 3rd party hand/finger tracking software: https://github.com/LightBuzz/Kinect-Finger-Tracking

2. 
The algorithm we came up with to delimit signs is called the "steady-active state algorithm" (SASA). 
SASA assumes that there will be less hand movement at the start and end of a sign.
SASA recognizes two hand states:
Active state - the hand is actively moving i.e. during signs
Steady state - the hand is relatively still i.e. between signs



