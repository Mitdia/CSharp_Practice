// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <mkl_df.h>
#include <iostream>

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}


extern "C" _declspec(dllexport)
int interpolate(int amount,
	double* segmentEnds, double* fieldValues,
	double* secondDerivativeOnSegmentEnds,
	int newNodesAmount, double* nodes,
	double* leftIntegralEnds, double* rightIntegralEnds,
	double* nodeValues, double* integralValues) {

	DFTaskPtr task;
	int error_code = dfdNewTask1D(&task, amount, segmentEnds, DF_UNIFORM_PARTITION, 2, fieldValues, DF_MATRIX_STORAGE_ROWS);
	if (error_code != DF_STATUS_OK) {
		return error_code;
	}

	double* coeff = new double[2 * DF_PP_CUBIC * (amount - 1)];
	error_code = dfdEditPPSpline1D(task,
		DF_PP_CUBIC, DF_PP_NATURAL,
		DF_BC_2ND_LEFT_DER | DF_BC_2ND_RIGHT_DER,
		secondDerivativeOnSegmentEnds,
		DF_NO_IC, NULL, coeff,
		DF_NO_HINT);
	if (error_code != DF_STATUS_OK) {
		dfDeleteTask(&task);
		delete[] coeff;
		return error_code;
	}
	delete[] coeff;

	error_code = dfdConstruct1D(task, DF_PP_SPLINE, DF_METHOD_STD);
	if (error_code != DF_STATUS_OK) {
		dfDeleteTask(&task);
		return error_code;
	}

	const int dorder[3] = { 1, 0, 1 };
	error_code = dfdInterpolate1D(task,
		DF_INTERP, DF_METHOD_PP,
		newNodesAmount, nodes, DF_SORTED_DATA,
		3, dorder, NULL, nodeValues,
		DF_MATRIX_STORAGE_ROWS, NULL);
	if (error_code != DF_STATUS_OK) {
		dfDeleteTask(&task);
		return error_code;
	}

	error_code = dfdIntegrate1D(task,
		DF_METHOD_PP, 1,
		leftIntegralEnds, DF_SORTED_DATA,
		rightIntegralEnds, DF_SORTED_DATA,
		NULL, NULL, integralValues, DF_MATRIX_STORAGE_ROWS);
	if (error_code != DF_STATUS_OK) {
		dfDeleteTask(&task);
		return error_code;
	}

	return error_code;
}


/*
extern "C" _declspec(dllexport)
int interpolate(int amount) {
	std::cout << amount;
	return 0;
}
*/