# Connect

Connect can translate American Sign Language (ASL) into English sentences. In order to solve this difficult problem, we 
broke it down into several components - they are listed below.

Components:

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
After delimiting one sign from another and pruning "empty" frames, we had sets of frames that each represents a word.
We trained a multi-layer neural network using sets of 3D points countouring the hand over ~20 frames spread over the entire duration of the sign. This was done on Azure ML. The actual training data consisted of 60 video clips for each sign; 20 of which were used for cross-validation. On average, the cross-validation data scored 98% on accuracy.

* Performance Considerations (pruning unecessary data): Recording and analyzing 3D video data was extremely taxing on our machine. Therefore,
we had to analyze only 20 frames of data per sign and send only a 3rd of the contour points for both training and testing to Azure ML.
* Platform: Data was inputted and outputted on a Windows app.

1. Code used for the 3rd party hand/finger tracking software: https://github.com/LightBuzz/Kinect-Finger-Tracking

2. The algorithm we came up with...
*

WIP
