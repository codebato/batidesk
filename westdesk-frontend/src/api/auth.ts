import { apiClient } from "./client";

interface LoginRequest {
  email: string;
  password: string;
}

interface LoginResponse {
  token: string;
}


export async function login(email: string, password: string): Promise<string> {
  const response = await apiClient.post<LoginResponse>("/auth/login", {
    email,
    password,
  } satisfies LoginRequest);

  return response.data.token;
}