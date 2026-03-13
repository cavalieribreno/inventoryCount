// centralized fetch wrapper that adds JWT token to all requests
export async function apiFetch(url: string, options: RequestInit = {}) {
    const saved = localStorage.getItem("user");
    const token = saved ? JSON.parse(saved).token : null;

    const headers: HeadersInit = {
        "Content-Type": "application/json",
        ...options.headers,
    };
    // add Authorization header if token exists
    if (token) {
        (headers as Record<string, string>)["Authorization"] = `Bearer ${token}`;
    }

    return fetch(url, { ...options, headers });
}
