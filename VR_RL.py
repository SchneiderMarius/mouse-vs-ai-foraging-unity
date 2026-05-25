import socket
import numpy as np
import os

class tcpsocket:
    def __init__(self, address):
        
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.sock.bind(('localhost', address))
        self.sock.listen(1)
        print('waiting for unity connection...') 
        
        self.conn, self.addr = self.sock.accept()
        print('unity connected!')

    def getresponse(self):
        data = self.conn.recv(1024)
        if not data:
            raise Exception("Empty message from Unity, socket disconnected")
        out = np.frombuffer(data,dtype='<f')
        #print("Python received:", out)
        return out

    #placeholder message
    def send(self):
        self.conn.sendall(b'aaaaa')

    #data message
    def sendvector(self,data):
        message = data.tobytes() #this is byte[]
        self.conn.sendall(message)

    def close(self):
        self.conn.close()
        self.sock.close()
import dxcam 
import mss 
import time
import cv2 #just for plotting images, not grabbing anything

#fastest, but windows only
class frame_dx:
    
    def __init__(self,region):
        self.region = region
        self.cam = dxcam.create(region=region,output_color="GRAY")

    def grab(self):
        
        img = self.cam.grab()
        
        #if windows hasn't flipped to a new frame yet, dxcam returns None. In that case, wait for a new one.
        while img is None:
            img = self.cam.grab()
            time.sleep(0.0005)
        
        frame = img[:,:,0].squeeze() #frames are grabbed with 3 grayscale channels, keep only one.  
        
        return frame

    def close(self):
        cv2.destroyAllWindows()
        self.cam.release()

#cross platform, but slow
class frame_mss:
    
    def __init__(self,region):
        self.region = region
        self.cam = mss.mss()

    def grab(self):
        img = np.asarray(self.cam.grab(self.region))
        frame = cv2.cvtColor(img, cv2.COLOR_RGBA2GRAY)
        return frame

    def close(self):
        cv2.destroyAllWindows()
        self.cam.close()



import torch
from torch import nn
import random

#seeding
def set_seed(seed):
    random.seed(seed)
    np.random.seed(seed)
    torch.manual_seed(seed)
    torch.cuda.manual_seed_all(seed)

seed = 25 #(the highest number)
set_seed(seed)

torch.backends.cudnn.benchmark = True


#get device for training
device = "cuda" if torch.cuda.is_available() else "cpu"
print(f"Using {device} device")

class FullyConnected(nn.Module):
    def __init__(self,input_dims):
        
        hidden_size = 512
        input_size = input_dims[0] * input_dims[1]
        output_size = 3
        
        super().__init__()
        self.flatten = nn.Flatten()
        self.fully_connected = nn.Sequential(
            nn.Flatten(0,-1), #changed to start_dim=0 as this isn't running on batches !!! when running me on batches change this !!!
            nn.Linear(input_size, hidden_size),
            nn.ReLU(),
            nn.Linear(hidden_size, hidden_size),
            nn.ReLU(),
            nn.Linear(hidden_size, output_size)
        )

    def forward(self, x):
        out = self.fully_connected(x)
        return out
    


## Main Set up
import subprocess
import tkinter

#get monitor resolution
root = tkinter.Tk()
width = root.winfo_screenwidth()
height = root.winfo_screenheight()
root.destroy() # Destroy the Tkinter window

#screen grab bounds
dims = np.array([155,86]) #game width, height
window_header = 11 #windows: remove title bar. Linux would need editing here.
origin = np.array([int(width/2 - dims[0]/2),int(height/2 - dims[1]/2)+window_header])

# #create frame grabber, dxcam. fastest but windows only.
region = (origin[0], origin[1], origin[0]+dims[0], origin[1]+dims[1])
frame_grabber = frame_dx(region)

#alternatively, mss frame grabber. slower but crossplatform.
# region = {"top":int(origin[1]), "left":int(origin[0]), "width":int(dims[0]), "height":int(dims[1])}    
# frame_grabber = frame_mss(region)

#create model
model = FullyConnected(dims).to(device)
print(model)

#start unity now!
unity_process = subprocess.Popen([r"C:\Users\BionicVisionVR\Documents\Mouse\starter_kits_robust_foraging\robust_foraging_windows_torch\Builds\RandomTrain\2D go to target v1.exe"])

#open socket
try:
    unity = tcpsocket(12345)
except socket.timeout:
    print('Connection to Unity timeout!')



## Main training loop
duration=1000 #make this as long as you want, or replace the iterator below with while true.

# Unity plots a few frames to get the screen updating, then waits for the blocking loop to start.
#
# 1. unity plots a frame, tells Python by sending the previous frame's reward, then waits for the next message
# 2. python reads that message, then grabs the frame and processes with the nn
# 3. python sends its next action to unity, then waits for the next message
# then the loop restarts. 
# 
# By the time this cell is started, step 1 is complete. Step 2 is the first action in the loop below.
# 
# The loop iterations are synced between python and unity. Both ends have blocking comms calls, so it stays synchronous.
# I've already validated that they stay synchronized in operation. 

now = time.perf_counter()

for i in range(duration):

    #wait for unity to flip a frame, grab the reward value from the last frame
    try:
        reward = unity.getresponse() #blocking
    except socket.timeout:
        print('Unity feedback timeout!')
        unity.close()
        break
    except Exception as e:
        print('Unexpected error:', e)
        break

    #grab a frame- this loops until windows flips to new frame
    frame = frame_grabber.grab() #this could also block for a very short period while waiting for windows to flip the screen.    
    # cv2.imshow('title',frame) #optionally plot the frame
    # cv2.waitKey(1)    
    
    #forward pass in model to get action
    input = torch.from_numpy(frame).float().to(device)
    with torch.no_grad():
        output = model(input)
    
    #send action to unity
    unity.sendvector(output.cpu().numpy())

    #for training: use "input", "action", and "reward" variables to construct (state,action,outcome)
    #note that you'll need input and action from the LAST iteration, plus reward from THIS iteration for an aligned sample

    #fps measurement
    if(i % 100 == 0 and i != 0):
        last = now
        now = time.perf_counter()
        print("fps:", 100 / (now-last)) #loops per second = process fps


# Clean up when finished.
print('Loop finished!')
frame_grabber.close()
unity.close()
del frame_grabber

unity_process.kill()