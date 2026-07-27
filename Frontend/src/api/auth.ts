import { isAxiosError } from "axios";
import { apiClient } from "./client";
import type { UserDTO } from "@/types/api";

export async function login(username: string, password: string): Promise<void> {
  try {
    await apiClient.post("/auth/login", { username, password });
  } catch (err) {
    if (isAxiosError(err) && err.response?.status === 401) {
      const message =
        typeof err.response.data === "string"
          ? err.response.data
          : "Invalid credentials.";
      throw new Error(message);
    }
    throw err;
  }
}

export async function register(
  username: string,
  password: string,
): Promise<void> {
  try {
    await apiClient.post("/auth/register", { username, password });
  } catch (err) {
    if (isAxiosError(err) && err.response?.status === 400) {
      const message =
        typeof err.response.data === "string"
          ? err.response.data
          : "Registration failed.";
      throw new Error(message);
    }
    throw err;
  }
}

export async function logout(): Promise<void> {
  await apiClient.post("/auth/logout");
}

export async function getMe(): Promise<UserDTO | null> {
  try {
    const response = await apiClient.get<UserDTO>("/auth/me");
    return response.data;
  } catch (err) {
    if (isAxiosError(err) && err.response?.status === 401) return null;
    throw err;
  }
}
