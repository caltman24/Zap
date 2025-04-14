export type JsonResponseParams<T> = {
  data?: T | null;
  error: string | null;
  headers?: HeadersInit;
};

export type JsonResponseResult<T> = {
  data?: T | null;
  error: string | null;
};

export type ActionResponseResult = {
  success: boolean;
  error: string | null;
};

export function JsonResponse<T>(params: JsonResponseParams<T>): Response {
  return Response.json(
    {
      data: params.data,
      error: params.error,
    },
    {
      headers: {
        ...params.headers,
      },
    }
  );
}

export function ActionResponse(params: ActionResponseResult): Response {
  return Response.json({
    ...params,
  });
}
