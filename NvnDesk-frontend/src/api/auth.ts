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
interface RegisterRequest {
  companyName: string;
  fullName: string;
  email: string;
  password: string;
}

interface RegisterResponse {
  token: string;
  email: string;
  fullName: string;
  role: string;
  tenantId: string;
}

export async function register(
  companyName: string,
  fullName: string,
  email: string,
  password: string
): Promise<string> {
  const response = await apiClient.post<RegisterResponse>("/auth/register", {
    companyName,
    fullName,
    email,
    password,
  } satisfies RegisterRequest);

  return response.data.token;
}