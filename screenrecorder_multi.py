import dxcam
from matplotlib import pyplot as plt
import cv2
from datetime import datetime, time
import numpy as np
import os
import sys
import scipy.io
import keyboard
import time
from multiprocessing import Process
import multiprocessing


#function for all recording machinery
#windows duplicator limitation requires separate instances to be in separate processes
#thus multiprocessing is required for 3 simultaneous recorders
def recorder(camnum,camnumoutput,target_fps,output_path): 
    
    #create cameras
    cam1 = dxcam.create(output_idx=camnum, output_color="BGR")

    #start cameras
    cam1.start(target_fps=target_fps,video_mode=True) 

    #outpaths for each video
    vidoutpath1 =  os.path.join(output_path, "screencam" + str(camnumoutput) + ".mp4") #output num is different in case windows numbering is stupid

    #writers for each video
    writer1 = cv2.VideoWriter(vidoutpath1,cv2.VideoWriter_fourcc(*"mp4v"),target_fps,(1024,600))

    #timestamp containment
    timestamps = []


    #record loop
    print("Recording start: ",datetime.now())
    print("Press ` key to end recording.")
    numframes = 0
    while(True):

        #grab a frame from every buffer.
        #Note: each get() call is blocking, thus code only proceeds once all cameras fill the buffer.
        numframes += 1
        timestamps.append(datetime.now())
        w1 = writer1.write(cam1.get_latest_frame())

        if(numframes % 20 == 0):
            print("\r",end="")
            print("Frames saved:", numframes,end=" ")

        #check for end keypress
        if keyboard.is_pressed('`'):  # if key '`q`' is pressed 
            print(" ")
            break


    #stop and release the resources
    cam1.release()
    del cam1
    writer1.release()

    print("Video saved to",vidoutpath1)


    #process timestamps for export
    timestamps = [d.strftime('%d/%m/%y %H:%M:%S.%f') for d in timestamps]

    #save it
    timestampoutpath = os.path.join(output_path, "timestamps" + str(camnumoutput) + ".txt")
    with open(timestampoutpath,"w") as f:
        for i in timestamps:
            f.write(i+"\n")
    print('Timestamps saved to', timestampoutpath)


if __name__ == '__main__':
    #input settings
    target_fps = 60
    output_path = input("Enter output directory (in quotes): ")
    output_path = output_path[1:-1] #remove quotes
    print("Saving files to ",output_path)

    #start recorder processes
    Process(target=recorder,args=(1,1,target_fps,output_path,)).start() #each recorder gets its own process, output numbering is distinct from windows numbering 
    #Process(target=recorder,args=(2,1,target_fps,output_path,)).start() #so center is 1 and left is 2
    #Process(target=recorder,args=(3,target_fps,output_path,)).start()