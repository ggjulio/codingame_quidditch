# **************************************************************************** #
#                                                                              #
#                                                         :::      ::::::::    #
#    algo.py                                            :+:      :+:    :+:    #
#                                                     +:+ +:+         +:+      #
#    By: juligonz <juligonz@student.42.fr>          +#+  +:+       +#+         #
#                                                 +#+#+#+#+#+   +#+            #
#    Created: 2020/04/14 17:56:29 by juligonz          #+#    #+#              #
#    Updated: 2020/04/14 17:58:11 by juligonz         ###   ########.fr        #
#                                                                              #
# **************************************************************************** #


import sys
import math
import numpy as np
import pandas as pd

# Grab Snaffles and try to throw them through the opponent's goal!
# Move towards a Snaffle to grab it and use your team id to determine towards where you need to throw it.
# Use the Wingardium spell to move things around at your leisure, the more magic you put it, the further they'll move.

# x, y: position
# vx: velocity
# vy: velocity
# entity_id: entity identifier
# entity_type: "WIZARD", "OPPONENT_WIZARD" or "SNAFFLE" or "BLUDGER"
# state: 1 if the wizard is holding a Snaffle, 0 otherwise. 1 if the Snaffle is being held, 0 otherwise. id of the last victim of the bludger.



my_team_id = int(input())  # if 0 you need to score on the right of the map, if 1 you need to score on the left
my_goal = (0, 3750) if my_team_id else (1600, 3750)
opponent_goal = (16000, 3750) if not my_team_id else (0, 3750) 

# df columns :
# 	entity_id,  entity_type,   state,    position,    velocity,
# 	dist_wizard0, dist_wizard1, dist_opponent2, dist_opponent3, 
# 	magnitude_wizard0, magnitude_wizard1, magnitude_opponent2, magnitude_opponent3

def game_loop():
	while True:
		my_score, my_magic = [int(i) for i in input().split()]
		opponent_score, opponent_magic = [int(i) for i in input().split()]
		nb_entities = int(input())
		df_entities = get_df_entities(nb_entities)
		df_entities = update_df(df_entities)
#		DEBUG(df_entities)
		# Actions
		for wizard_id in range(2):
			wizard_step(wizard_id, df_entities)

def attack_logic(wizard_id, df):
	magnitude_col = 'magnitude_wizard' + str(wizard_id)
	df_result = get_df_by_type(df, 'SNAFFLE') \
		.sort_values(by=[magnitude_col], ascending=[True])
#	DEBUG(f"wizard id : {wizard_id}")
#	DEBUG(df.loc[wizard_id].state)
	if df.loc[wizard_id].state == 1:
		throw(opponent_goal, 100)
		return
#	DEBUG(df_result)
	df_result = df_result[df_result['owner'] == -1] # if SNAFFLE not inside player circle
	#DEBUG(df_result)
	df_result = df_result[np.array(df_result['dist_wizard' + str(wizard_id)].tolist())[:,0] < 0]

	move(df_result.iloc[0].position, 100)

def wizard_step(wizard_id, df):
	wizard = df.loc[wizard_id]
#	if wizard_id == 1 :
	attack_logic(wizard_id, df)
#	else:
#		move([8000, 3750], 100)

def get_df_by_type(df, type):
	return(df[df['entity_type'] == type])

def update_df(df):
	df['position'] = [np.array([row['x'],row['y']]) for x, row in df.iterrows()]
	df['velocity'] = [np.array([row['vx'],row['vy']]) for x, row in df.iterrows()]
	df['dist_wizard0'] = [df.iloc[0].position - row['position'] for idx, row in df.iterrows()]
	df['dist_wizard1'] = [df.iloc[1].position - row['position'] for idx, row in df.iterrows()]
	df['dist_opponent2'] = [df.iloc[2].position - row['position'] for idx, row in df.iterrows()]
	df['dist_opponent3'] = [df.iloc[3].position - row['position'] for idx, row in df.iterrows()]
	df['magnitude_wizard0'] = [np.linalg.norm(row['dist_wizard0']) for idx, row in df.iterrows()]
	df['magnitude_wizard1'] = [np.linalg.norm(row['dist_wizard1']) for idx, row in df.iterrows()]
	df['magnitude_opponent2'] = [np.linalg.norm(row['dist_opponent2']) for idx, row in df.iterrows()]
	df['magnitude_opponent3'] = [np.linalg.norm(row['dist_opponent3']) for idx, row in df.iterrows()]
	# state for snaffle, wizard have snaffle if distance lest than 400. the value equal the id of actual owner
	lst = []
	for x, row in df.iterrows():
		if row.magnitude_wizard0 < 400 :
			lst.append(0)
		elif row.magnitude_wizard1 < 400:
			lst.append(1)
		elif row.magnitude_opponent2 < 400 :
			lst.append(2)
		elif row.magnitude_opponent3  < 400 :
			lst.append(3)
		else:
			lst.append(-1)
	df['owner'] = lst
	df = df.drop(columns=['x','y', 'vx', 'vy'])
	return(df)

def get_df_entities(nb_entities):
	arr = []
	for i in range(nb_entities):
		arr.append(input().split())
	df = pd.DataFrame(arr, columns=['entity_id','entity_type','x','y','vx','vy','state'])
	col_int_type = ['entity_id','x','y','vx','vy','state']
	df[col_int_type] = df[col_int_type].apply(pd.to_numeric)
	df = df.set_index('entity_id')
	return (df)

# 0 ≤ thrust ≤ 150
def move(vector, thrust):
	print(f"MOVE {vector[0]} {vector[1]} {thrust}", flush=True)

# (0 ≤ power ≤ 500)
def throw(vector, power):
	print(f"THROW {vector[0]} {vector[1]} {power}")

# (0 ≤ magic ≤ 1500)
def wingardium(wizard_id, x, y, magic):
	print(f"WINGARDIUM {wizard_id} {x} {y} {magic}")



# To debug: print("Debug messages...", file=sys.stderr)
def DEBUG(x):
	pd.set_option('display.max_columns', None)
	pd.set_option('display.width', 800)
	print(x, file=sys.stderr)

game_loop()

