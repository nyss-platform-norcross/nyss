export class RequestError extends Error {
  constructor(message, data) {
    super(message);
    this.name = "RequestError";
    this.data = data;
  }
}
