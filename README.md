# Connect

Connect can translate American Sign Language (ASL) into English sentences. In order to solve this problem, we 
broke it down into several components - they are listed below.

Disclaimer: Since this app was written by two people at a Hackathon, the code is messy and disorganized. However, a lot of thoughts were put into the actual algorithms used. <br/>

### Components:

* High Quality Video Capture (gather raw input data): 
The Microsoft Kinect is able to capture high quality, depth-sensitive video data.
This allows for easier hand/finger recongition later on.

* Finger/Hand Tracking (extracting relevant data): 
Although some signs in ASL use facial expressions as "part of the sign", signs generally utilize the hands and fingers exclusively. For this project, we only considered the hands and fingers of the sign-er. A 3rd party algorithm was used to draw contours around the hands and fingers (a contour is a set of points outlining the hands/fingers).[1]
Each frame in the video was mapped to a set of 3D points which outline the fingers/hand.

* Delimiting Signs (semantic filtering/grouping of data): 
There are no specific delimiters used in ASL to separate one sign (word) from another.
For Connect, we made the assumption that hands slow down/stop at the start and end of a sign. We created an algorithm that is able to
consistently separate one word from another based on this assumption.[2]
Using this algorithm, contiguous frames were grouped together when they were believed to be part of one word/sign. 
Frames where no sign was detected were labelled as "empty frames" and were pruned.

* Sign-to-word Prediction (interpreting data): 
By delimiting one sign from another and pruning "empty" frames in between each sign, we were able to create several ordered sets. Each set contained 3D contour points, ordered based on the timestamp of the frame that the contour originated from. These 3D contour points were the features that we used for our machine learning. However, the number of elements were inconsistent across the sets. Therefore, for each frame, we only included a tenth of the contour points (about a third of the total points). In addition, we only used 20 frames from the entire duration of the sign. These frames were chosen such that the sign was broken up into equal intervals. With the above modifications, each and every sign now had a consistent number of 3D points/features. Using Azure ML, we trained a multi-layer neural network using 60 video clips for each sign; 20 of which were used for cross-validation. On average, the cross-validation data scored 98% on accuracy.

* Performance Considerations (pruning unnecessary data): 
Recording and analyzing 3D video data was extremely taxing on our workstation. Even with using only 20 frames and a tenth of the contour points for each sign, the performance of our application was laggy. Performance improvements are likely with a better computer.

1. 
Code used for the 3rd party hand/finger tracking software: https://github.com/LightBuzz/Kinect-Finger-Tracking

2. 
The algorithm we came up with...
*

