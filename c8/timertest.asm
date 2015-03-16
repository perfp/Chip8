		CLS
		LD I, #000
		LD V3, #03
		LD V4, #03
		DRW V3, V4, 5
		LD V0, #FF ; 4335 ms 
		LD DT, V0
LOOP:	LD V1, DT
		SE V1, #00
		JP LOOP
		LD I, #005
		DRW V3, V4, 5
		RET

