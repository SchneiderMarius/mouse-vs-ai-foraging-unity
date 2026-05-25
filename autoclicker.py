
import time
from datetime import datetime, timedelta

import os
import signal
from collections import Counter

import pyautogui


'''
FILE_PATH: txt log file path
THRESHOLD_TIME: time frequency for checking the file
    THRESHOLD_TIME = 10 means check the log file every 10 seconds
PATIENCE: when the success rate isnt improving, how long we should wait before ending the entire experimenet
    This waiting time can be calcualted by PATIENCE * THRESHOLD_TIME:
    Suppose we check file every 1800 seconds and the PATIENCE=5, 
    then we wait for 1800 seconds * 5 patience = 9000 seconds, 
    if the success rate doesnt improve within this 9000 seconds,
    we end the experiment
MAX_TRAINING_TIME: the maxium duration of the experiment
WAITING_TIME: warm up stage, we only evalute the success rate after the waiting time has passed
    For example, if the WAITING_TIME is 3 hours, 
    this means we dont evaluate the success rate and let the model run for the first 3 hours; 
    after 3 hours has passed, we then evalue the success 
    
    rate and do early stopping
    
Note:
    lower THRESHOLD_TIME == checking files more often
    WAITING_TIME and THRESHOLD_TIME must be smaller than MAX_TRAINING_TIME
'''

FILE_PATH = 'C:/Users/BionicVisionVR/Documents/Mouse/mouse - test res/6.12-fc-marius-10times.txt' 
THRESHOLD_TIME = 10*60  # unit: seconds
PATIENCE = 5 
MAX_TRAINING_TIME = 2*60*60  # unit: seconds
WAITING_TIME = 10*60  # unit: seconds


def parse_log_line(line, base_date):
    """Parses a log line and returns a dictionary with session time and other details."""
    parts = line.strip().split('\t')
    if len(parts) > 1:
        
        session_time_str = parts[0]  # Time is in the first part
        event_details = parts[1:]  # Other event details
        if len(session_time_str.split(':'))>2:
            # Parse time
            session_time = datetime.strptime(session_time_str, "%H:%M:%S.%f").time()

            # Combine with base date to get a full datetime
            full_datetime = datetime.combine(base_date, session_time)

            return {
                'datetime': full_datetime,
                'details': event_details
            }
    return None


def read_second_last_line(file_path):
    """Reads the second-to-last line of a file (to avoid trunked line)"""
    with open(file_path, 'rb') as f:
        f.seek(-2, 2)  # Move to the second last byte in the file
        while f.read(1) != b'\n':  # Skip the last line
            f.seek(-2, 1)
        f.seek(-2, 1)  # Skip over the newline at the end of the second-to-last line
        while f.read(1) != b'\n':  # Read until the start of the second-to-last line
            f.seek(-2, 1)
        return f.readline().decode().strip()  # Read and decode the second-to-last line


def find_lines_within_thres(file_path, target_time, threshold_time):
    """Finds all lines in the file within threshold_time seconds of the target time."""
    lines_within_hour = []
    base_date = target_time.date()  
    with open(file_path, 'r') as f:
        for line in f:
            parsed = parse_log_line(line, base_date)
            if parsed:
                line_time = parsed['datetime']

                if line_time > target_time:
                    line_time -= timedelta(days=1)

                if abs((line_time - target_time).total_seconds()) <= threshold_time:
                    lines_within_hour.append(line.strip())

    return lines_within_hour





best_success_rate = 0.0
prev_last_line = ''
ct = 0

print(f"Check lines within {THRESHOLD_TIME} seonds")

start_time = datetime.now()
end_time = start_time + timedelta(seconds=MAX_TRAINING_TIME)
eval_time = start_time + timedelta(seconds=WAITING_TIME)

if eval_time > end_time: print(f'WAITING_TIME {WAITING_TIME} must be smaller than MAX_TRAINING_TIME {MAX_TRAINING_TIME}')
current_time = datetime.now()


while current_time < end_time:
    if current_time < eval_time:
        time.sleep(THRESHOLD_TIME) 
        current_time = datetime.now()
        print(f'Model warm up stage {current_time}, dont evaluate success rate until {eval_time}')
    else: break
        
        
while current_time < end_time:
    print('Start evaluating')
    last_line = read_second_last_line(FILE_PATH)
    if last_line != prev_last_line: prev_last_line = last_line
    else: 
        print('Experiment has stopped, exit script')
        pyautogui.hotkey('ctrl', 'c')
        pyautogui.hotkey('ctrl', 'c')
        pyautogui.hotkey('ctrl', 'c')
                        
        break

    base_date = datetime.now().date()  
    last_line_parsed = parse_log_line(last_line, base_date)

    res = []
    if last_line_parsed:
        target_time = last_line_parsed['datetime']
        if target_time > datetime.now():
            target_time -= timedelta(days=1)
                    
        print(f"Last line time: {target_time}")
        lines_within_thres = find_lines_within_thres(FILE_PATH, target_time,THRESHOLD_TIME)
        
        for line in lines_within_thres:
            if len(line.split('\t')) > 1:
                if line.split('\t')[1] == 'h' or line.split('\t')[1] == 'f':
                    res.append(line.split('\t')[1])
        if len(res) != 0 :
            trial = Counter(res)
            if 'h' in trial.keys(): h = trial['h']
            else: h = 0
            if 'f' in trial.keys(): f = trial['f']
            else: f = 0
            if h + f == 0: print('no trials recorded; possible error in log file') 
            else: 
                success_rate = h / (h + f)
                print(f'Current performance: {trial}; current success rate: {round(success_rate,3)}; best success rate: {round(best_success_rate,3)}')
                if success_rate > best_success_rate:
                    best_success_rate = success_rate
                    ct = 0
                else:
                    ct += 1
                    print(f"Success rate hasn't improved for {ct}/{PATIENCE} times")
                    if ct >= PATIENCE:
                        print('Max patience reached, exit training')
                        pyautogui.hotkey('ctrl', 'c')
                        pyautogui.hotkey('ctrl', 'c')
                        pyautogui.hotkey('ctrl', 'c')
                        
                        break
    else:
        print("Could not parse the last line.")
        
    time.sleep(THRESHOLD_TIME) 
    current_time = datetime.now()
    
print('Max training time reached, exit training')
pyautogui.hotkey('ctrl', 'c')
pyautogui.hotkey('ctrl', 'c')
pyautogui.hotkey('ctrl', 'c')


